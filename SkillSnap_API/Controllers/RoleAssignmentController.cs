using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap_API.Controllers;

/// <summary>
/// Handles assigning and removing Identity roles for ApplicationUser accounts.
/// Only Admin users should be allowed to access these endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoleAssignmentController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleAssignmentController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Assigns an IdentityRole to a user by userId and roleName.
    /// </summary>
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            return BadRequest("UserId and RoleName are required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with ID '{userId}' not found.");

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
            return BadRequest($"Role '{roleName}' does not exist.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return StatusCode(500, result.Errors);

        return Ok($"Role '{roleName}' assigned to user '{user.Email}'.");
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            return BadRequest("UserId and RoleName are required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with ID '{userId}' not found.");

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
            return BadRequest($"Role '{roleName}' does not exist.");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return StatusCode(500, result.Errors);

        return Ok($"Role '{roleName}' removed from user '{user.Email}'.");
    }

    /// <summary>
    /// Returns a list of Identity roles assigned to a user.
    /// </summary>
    [HttpGet("get-roles/{userId}")]
    public async Task<IActionResult> GetRoles(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with ID '{userId}' not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(roles);
    }

    /// <summary>
    /// Returns a list of all available roles in the system.
    /// </summary>
    [HttpGet("all-roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Updates all roles for a user by replacing their current roles with the provided list.
    /// </summary>
    [HttpPost("update-roles")]
    public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] IEnumerable<string> rolesToAssign)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required.");

        if (rolesToAssign == null)
            return BadRequest("Roles list is required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with ID '{userId}' not found.");

        // Get current roles and remove all of them
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return StatusCode(500, new { message = "Failed to remove current roles", errors = removeResult.Errors });

        // Add new roles
        var rolesToAssignList = rolesToAssign.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (rolesToAssignList.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAssignList);
            if (!addResult.Succeeded)
                return StatusCode(500, new { message = "Failed to assign new roles", errors = addResult.Errors });
        }

        return Ok(new { message = $"Roles updated successfully for user '{user.Email}'.", roles = rolesToAssignList });
    }
}


