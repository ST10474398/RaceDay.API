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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Both Organisers and Participants can view all events
        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        // ONLY Admin or Organiser can create events
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            // 1. Check if the Category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == newEvent.CategoryID);
            if (!categoryExists)
            {
                return NotFound($"Category with ID {newEvent.CategoryID} does not exist. Please create a category first.");
            }

            // 2. Get the logged-in user's ID
            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(organiserId))
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            // 3. Set the values
            newEvent.OrganiserID = organiserId;
            newEvent.CreatedAt = DateTime.UtcNow;
            newEvent.Status = "Upcoming";

            // 4. Save to database
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Event created successfully!", EventId = newEvent.EventID });
        }

        // PUT: api/Events/5
        // ONLY Admin or Organiser can update events
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound("Event not found.");

            // Update the fields
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
        // ONLY Admin or Organiser can delete events
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null) return NotFound("Event not found.");

            // Prevent deletion if there are enrolments
            if (eventItem.Enrolments.Any())
            {
                return Conflict("Cannot delete an event that already has enrolments.");
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Event deleted successfully!" });
        }

        // GET: api/Events/5/enrolments
        // Admin/Organiser can view all enrolments for a specific event
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
    }
}