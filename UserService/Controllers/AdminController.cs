using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly UserDbContext _context;

        public AdminController(UserDbContext context)
        {
            _context = context;
        }

        // GET /api/admin/users - Lấy tất cả users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users);
        }

        // DELETE /api/admin/users/{id} - Xóa user
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent admin from deleting themselves
            var currentUserIdClaim = User.FindFirst("userId");
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                if (currentUserId == id)
                {
                    return BadRequest(new { message = "Cannot delete your own account" });
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/admin/users/stats - Thống kê users
        [HttpGet("users/stats")]
        public async Task<IActionResult> GetUserStats()
        {
            // Check if user is admin
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                return Forbid();
            }

            var totalUsers = await _context.Users.CountAsync();
            var todayUsers = await _context.Users
                .Where(u => u.CreatedAt.Date == DateTime.UtcNow.Date)
                .CountAsync();

            return Ok(new
            {
                totalUsers,
                todayUsers
            });
        }
    }
}
