using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Enrolments
        // Any logged-in user can enrol in an event
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnrolInEvent([FromBody] EnrolmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 1. Check if event exists
            var raceEvent = await _context.Events.FindAsync(request.EventId);
            if (raceEvent == null) return NotFound("Event not found.");

            // 2. Check if event is full
            var currentEnrolments = await _context.EventEnrollments
                .CountAsync(en => en.EventID == request.EventId && en.Status != "Cancelled");

            if (currentEnrolments >= raceEvent.MaxParticipants)
            {
                return BadRequest("This event is already full.");
            }

            // 3. Check if user is already enrolled (unique constraint)
            var existingEnrolment = await _context.EventEnrollments
                .FirstOrDefaultAsync(en => en.EventID == request.EventId && en.UserID == userId);

            if (existingEnrolment != null)
            {
                return Conflict("You are already enrolled in this event.");
            }

            // 4. Create the enrolment with Pending status
            var enrolment = new EventEnrollment
            {
                EventID = request.EventId,
                UserID = userId,
                Status = "Pending",
                EnrolledAt = DateTime.UtcNow
            };

            _context.EventEnrollments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Enrolment successful. Awaiting approval.", EnrolmentID = enrolment.EnrolmentID });
        }

        // GET: api/Enrolments/my
        // Any logged-in user can view their own enrolments
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var enrolments = await _context.EventEnrollments
                .Include(en => en.Event)
                .Include(en => en.Result)
                .Where(en => en.UserID == userId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/event/5
        // Admin/Organiser can view all enrolments for a specific event
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var raceEvent = await _context.Events.FindAsync(eventId);
            if (raceEvent == null) return NotFound("Event not found.");

            var enrolments = await _context.EventEnrollments
                .Include(en => en.User)
                .Where(en => en.EventID == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: api/Enrolments/5/status
        // Admin/Organiser can approve or reject an enrolment
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateEnrolmentStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            var enrolment = await _context.EventEnrollments.FindAsync(id);
            if (enrolment == null) return NotFound("Enrolment not found.");

            // Validate status
            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest("Invalid status. Must be Pending, Approved, Rejected, or Cancelled.");
            }

            enrolment.Status = request.Status;
            enrolment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Enrolment status updated to {request.Status}." });
        }

        // DELETE: api/Enrolments/5
        // A user can cancel their OWN enrolment
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var enrolment = await _context.EventEnrollments.FindAsync(id);
            if (enrolment == null) return NotFound("Enrolment not found.");

            // Only the owner or an admin can cancel
            if (enrolment.UserID != userId && !isAdmin)
            {
                return Forbid("You can only cancel your own enrolments.");
            }

            enrolment.Status = "Cancelled";
            enrolment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Enrolment cancelled successfully." });
        }
    }

    // DTOs for the enrolments endpoints
    public class EnrolmentRequest
    {
        public int EventId { get; set; }
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}