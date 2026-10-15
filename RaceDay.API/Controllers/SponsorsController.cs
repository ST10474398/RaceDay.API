using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SponsorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SponsorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Sponsors
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllSponsors()
        {
            var sponsors = await _context.Sponsors.ToListAsync();
            return Ok(sponsors);
        }

        // GET: api/Sponsors/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSponsorById(int id)
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null) return NotFound("Sponsor not found.");
            return Ok(sponsor);
        }

        // POST: api/Sponsors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSponsor([FromBody] Sponsor newSponsor)
        {
            newSponsor.CreatedAt = DateTime.UtcNow;
            _context.Sponsors.Add(newSponsor);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Sponsor created.", SponsorID = newSponsor.SponsorID });
        }

        // PUT: api/Sponsors/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSponsor(int id, [FromBody] Sponsor updated)
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null) return NotFound("Sponsor not found.");

            sponsor.Name = updated.Name;
            sponsor.LogoUrl = updated.LogoUrl;
            sponsor.Website = updated.Website;
            sponsor.ContactEmail = updated.ContactEmail;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Sponsor updated." });
        }

        // DELETE: api/Sponsors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSponsor(int id)
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null) return NotFound("Sponsor not found.");

            _context.Sponsors.Remove(sponsor);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Sponsor deleted." });
        }
    }
}