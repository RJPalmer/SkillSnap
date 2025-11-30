using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillSnap.Shared.Models;
using SkillSnap_Shared.DTOs.Account;

namespace SkillSnap_API.Controllers;

/// <summary>
/// Admin endpoints for managing users.
/// Restricted to users in the Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Returns all users as AdminUserDto, including roles and last login.
    /// GET: /api/admin/users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = _userManager.Users.ToList();
        var result = new List<AdminUserDto>();

        foreach (var user in users)
        {
            // Retrieve roles for each user
            var roles = await _userManager.GetRolesAsync(user);

            // Map to DTO
            result.Add(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                // Use LastLogin if tracked elsewhere; LockoutEnd used here as placeholder
                LastLogin = user.LockoutEnd?.UtcDateTime,
                Roles = roles.ToList()
            });
        }

        return Ok(result);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            LastLogin = user.LockoutEnd?.UtcDateTime,
            Roles = roles.ToList()
        };

        return Ok(dto);
    }
}
