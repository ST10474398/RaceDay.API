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
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Results
        // Organiser/Admin records a result
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] ResultDto request)
        {
            // 1. Verify the enrolment exists
            var enrolment = await _context.EventEnrollments.FindAsync(request.EnrolmentID);
            if (enrolment == null) return NotFound("Enrolment not found.");

            // 2. Check if a result already exists for this enrolment (one-to-one)
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == request.EnrolmentID);
            if (existingResult != null)
            {
                return Conflict("A result already exists for this enrolment.");
            }

            var result = new Result
            {
                EnrolmentID = request.EnrolmentID,
                FinishTime = request.FinishTime,
                Position = request.Position,
                RecordedAt = DateTime.UtcNow,
                IsVerified = request.IsVerified
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Result recorded successfully.", ResultID = result.ResultID });
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetResultById(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                .FirstOrDefaultAsync(r => r.ResultID == id);
            if (result == null) return NotFound("Result not found.");
            return Ok(result);
        }

        // GET: api/Results/event/5
        [HttpGet("event/{eventId}")]
        [Authorize]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.User)
                .Where(r => r.Enrolment!.EventID == eventId)
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/me
        // A participant views their own results
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                .Where(r => r.Enrolment!.UserID == userId)
                .ToListAsync();

            return Ok(results);
        }

        // PUT: api/Results/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDto request)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound("Result not found.");

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.IsVerified = request.IsVerified;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Result updated successfully." });
        }

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound("Result not found.");

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Result deleted successfully." });
        }
    }

    public class ResultDto
    {
        public int EnrolmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}