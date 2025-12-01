using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SkillSnap.Shared.Models;
using SkillSnap.Shared.DTOs;
using SkillSnap_Shared.DTOs.Account;
using SkillSnap_API.Services;
using SkillSnap_API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SkillSnap_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _tokenService;
    private readonly SkillSnapDbContext _context;
    private readonly IConfiguration _config;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService tokenService,
        SkillSnapDbContext context,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        ApplicationUser? user = null;
        try
        {
            // Create Identity user
            user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            
            // Assign "User" role
            await _userManager.AddToRoleAsync(user, "User");
            
            // Create PortfolioUser record
            var portfolioUser = new PortfolioUser
            {
                ProfileImageUrl = string.Empty,
                Bio = string.Empty,
                Name = dto.Name,
                ApplicationUserId = user.Id
            };

            _context.PortfolioUsers.Add(portfolioUser);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Commit transaction
            await transaction.CommitAsync();

            return Ok(new { Token = token });
        }
        catch (Exception)
        {
            // Cleanup partially created Identity user if needed
            if (user != null)
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser != null)
                {
                    await _userManager.DeleteAsync(existingUser);
                }
            }
            await transaction.RollbackAsync();
            throw;
        }
    }



    // ===========================
    // LOGIN (returns JWT)
    // ===========================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return Unauthorized("Invalid email or password");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, false);

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");
        
         //update last login time
        //  = DateTime.UtcNow;
        // await _userManager.UpdateAsync(user);
        var portfolioUser = await _context.PortfolioUsers
            .FirstOrDefaultAsync(pu => pu.ApplicationUserId == user.Id);
        if (portfolioUser != null)
        {
            user.PortfolioUser = portfolioUser;
        }
        
        // Generate JWT Token
        var token = await _tokenService.GenerateToken(user);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60"))
        });

       
    }

    // ===========================
    // LOGOUT (Works with Identity cookies)
    // JWT logout is client-side only
    // ===========================
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new { Message = "Logged out successfully" });
    }

    [Authorize]
    [HttpPost("link-portfolio-user")]
    public async Task<IActionResult> LinkPortfolioUser([FromBody] int portfolioUserId)
    {
        var appUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(appUserId))
            return Unauthorized("User not found in JWT claims.");

        var appUser = await _userManager.FindByIdAsync(appUserId);

        if (appUser == null)
            return Unauthorized("ApplicationUser not found.");

        if(appUser.PortfolioUser is not null && appUser.PortfolioUser.Id != portfolioUserId)
            return BadRequest("ApplicationUser is already linked to a PortfolioUser."); 

        var portfolioUser = await _context.PortfolioUsers.FindAsync(portfolioUserId);
        if (portfolioUser == null)
            return NotFound("PortfolioUser not found.");
        appUser.PortfolioUser = portfolioUser;

        var result = await _userManager.UpdateAsync(appUser);

        if (!result.Succeeded)
            return BadRequest("Failed to link PortfolioUser to ApplicationUser.");

        // Issue new JWT with updated claim
        var token = _tokenService.GenerateToken(appUser);

        return Ok(new { token });
    }
}