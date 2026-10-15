using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/Users/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.IsActive,
                user.CreatedAt
            });
        }

        // PUT: api/Users/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            user.FullName = model.FullName;

            if (user.Email != model.Email)
            {
                // Check if the new email is already taken
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return Conflict("That email is already in use.");
                }
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { Message = "Profile updated successfully." });
        }

        // GET: api/Users
        // Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.IsActive,
                u.CreatedAt
            });
            return Ok(userList);
        }

        // GET: api/Users/5
        // Admin only
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.IsActive,
                user.CreatedAt
            });
        }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}