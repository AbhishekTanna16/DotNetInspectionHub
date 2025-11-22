using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShopInspector.Web.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        try
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Login view");
            throw;
        }
    }
    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        try {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
     

        var isValid = username == "admin" && password == "password";
        if (!isValid)
        {
            _logger.LogWarning("Invalid login attempt for {User}", username);
            ModelState.AddModelError("", "Invalid username or password.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        _logger.LogInformation("User {User} logged in.", username);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Login attempt for {User}", username);
            throw;
        }

    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                _logger.LogInformation("User {User} logged out.", User.Identity!.Name);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogError("Logout Successfully ", User.Identity!.Name);
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Logout", User.Identity!.Name);
            throw;
        }
    }
}