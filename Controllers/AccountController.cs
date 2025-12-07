using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartGearOnline.Models;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }
        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, false);
        _logger.LogInformation("User registered {Email}", model.Email);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // find user by email first so we sign in with the correct user name
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            // use CheckPasswordSignInAsync to avoid relying on username strings
            var check = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            _logger.LogInformation("Login attempt for {Email} - Succeeded:{Succeeded} LockedOut:{LockedOut} NotAllowed:{NotAllowed} Requires2FA:{Requires2FA}",
                model.Email, check.Succeeded, check.IsLockedOut, check.IsNotAllowed, check.RequiresTwoFactor);

            if (check.Succeeded)
            {
                await _signInManager.SignInAsync(user, model.RememberMe);
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    // keep your ReturnUrl safety checks here
                    if (model.ReturnUrl.Contains("/products/create", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Admin"))
                            return RedirectToAction("Index", "Home");
                    }
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (check.IsLockedOut)
                ModelState.AddModelError("", "Account locked out.");
            else if (check.IsNotAllowed)
                ModelState.AddModelError("", "Sign-in is not allowed for this account.");
            else if (check.RequiresTwoFactor)
                return RedirectToAction("LoginWith2fa", new { model.ReturnUrl, model.RememberMe });
            else
                ModelState.AddModelError("", "Invalid login attempt.");

            return View(model);
        }

        // fallback when email not found
        _logger.LogInformation("Login attempt for unknown email {Email}", model.Email);
        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
}