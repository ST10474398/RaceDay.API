using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.IO; // Required for Path and FileStream
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organiser)
                .ToListAsync();
            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null) return NotFound("Event not found.");

            return Ok(eventItem);
        }

        // GET: api/Events/upcoming
        [HttpGet("upcoming")]
        [Authorize]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var events = await _context.Events
                .Where(e => e.EventDate > DateTime.UtcNow && e.Status == "Upcoming")
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/past
        [HttpGet("past")]
        [Authorize]
        public async Task<IActionResult> GetPastEvents()
        {
            var events = await _context.Events
                .Where(e => e.EventDate < DateTime.UtcNow || e.Status == "Completed")
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/Events
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(organiserId))
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == newEvent.CategoryID);
            if (!categoryExists)
            {
                return NotFound($"Category with ID {newEvent.CategoryID} does not exist. Please create a category first.");
            }

            newEvent.OrganiserID = organiserId;
            newEvent.CreatedAt = DateTime.UtcNow;
            newEvent.Status = "Upcoming";

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Event created successfully!", EventId = newEvent.EventID });
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound("Event not found.");

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.MaxParticipants = updatedEvent.MaxParticipants;
            existingEvent.CategoryID = updatedEvent.CategoryID;
            existingEvent.Status = updatedEvent.Status;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Event updated successfully!" });
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null) return NotFound("Event not found.");

            if (eventItem.Enrolments.Any())
            {
                return Conflict("Cannot delete an event that already has enrolments.");
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Event deleted successfully!" });
        }

        // GET: api/Events/5/enrolments
        [HttpGet("{id}/enrolments")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);
            if (raceEvent == null) return NotFound("Event not found.");

            var enrolments = await _context.EventEnrollments
                .Include(en => en.User)
                .Where(en => en.EventID == id)
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/Events/5/enrol
        [HttpPost("{id}/enrol")]
        [Authorize]
        public async Task<IActionResult> EnrolInEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You must be logged in to enrol.");
            }

            var raceEvent = await _context.Events.FindAsync(id);
            if (raceEvent == null) return NotFound("Event not found.");

            var existingEnrolment = await _context.EventEnrollments
                .FirstOrDefaultAsync(en => en.EventID == id && en.UserID == userId);

            if (existingEnrolment != null)
            {
                return Conflict("You are already enrolled in this event.");
            }

            var currentEnrolments = await _context.EventEnrollments
                .CountAsync(en => en.EventID == id && en.Status != "Cancelled");

            if (currentEnrolments >= raceEvent.MaxParticipants)
            {
                return BadRequest("This event is already full.");
            }

            var enrolment = new EventEnrollment
            {
                EventID = id,
                UserID = userId,
                Status = "Pending",
                EnrolledAt = DateTime.UtcNow
            };

            _context.EventEnrollments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Enrolment successful. Awaiting approval.", EnrolmentID = enrolment.EnrolmentID });
        }

        // ---------------------------------------------------------
        // LOCAL IMAGE UPLOAD (Replaces Azure)
        // ---------------------------------------------------------
        // POST: api/Events/upload-image
        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UploadEventImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 1. Create the folder path (wwwroot/images inside the API project)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 2. Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 3. Save the file to the local folder
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Return the URL path (relative to the API)
            var imageUrl = $"/images/{fileName}";
            return Ok(new { ImageUrl = imageUrl });
        }
    }
}