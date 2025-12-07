using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using SmartGearOnline.Models;

[ApiController]
[Route("dev")]
public class DevToolsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHostEnvironment _env;

    public DevToolsController(UserManager<ApplicationUser> userManager, IHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;
    }

    // Development-only: reset admin password and confirm email
    [HttpPost("reset-admin-password")]
    public async Task<IActionResult> ResetAdminPassword([FromQuery] string email = "admin@example.com", [FromQuery] string password = "Admin123!")
    {
        if (!_env.IsDevelopment()) return Forbid();

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound("user not found");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded) return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Ok("password reset");
    }
}