using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly IHttpContextAccessor _context;

    public LogoutModel(ILogger<LogoutModel> logger, IHttpContextAccessor context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out");
        
        return RedirectToAction("/");
    }
}
