using DBfirst.Data.Roles;
using DBfirst.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DBfirst.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly Project_B5DBContext _context;
        public AdminController(Project_B5DBContext context)
        {
            _context = context;
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("viewactive")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.User.Select(u => new
            {
                Username = u.UserName,
                Active = u.IsActive
            }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpPut("editactive")]
        public async Task<IActionResult> UpdateUserActiveStatus(string username, bool isActive)
        {
            // Find the user by username
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == username);

            // If user is not found, return NotFound
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Update the user's active status
            user.IsActive = isActive;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User's active status updated successfully." });
        }
    }
}
