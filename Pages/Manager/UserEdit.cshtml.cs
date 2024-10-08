using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

[Authorize]
public class UserEditModel : PageModel
{
    private readonly ILogger<UserEditModel> _logger;
    private readonly DbContextModel _context;

    public UserEditModel(ILogger<UserEditModel> logger, DbContextModel context)
    {
        _logger = logger;
        _context = context;
    }

    public User UserManager { get; set; }
    private static readonly List<string> AllowedFields = new List<string> { "name", "email", "password" };
    public async Task<IActionResult> OnGetAsync(string what, string email)
    {
        if (String.IsNullOrEmpty(what) || String.IsNullOrEmpty(email))
        {
            _logger.LogCritical("Url data is empty");
            return Redirect("http://localhost:5229/home");
        }

        if (!AllowedFields.Contains(what))
        {
            _logger.LogCritical("Url isn't allowed");
            return Redirect("http://localhost:5229/home");
        }

        if (what == null)
        {
            _logger.LogCritical($"Invalid parameter in url");
            return Redirect("http://localhost:5229/home");
        }

        if (!IsAuthenticated(email))
        {
            _logger.LogCritical("User isn't authenticate");
            return Redirect("http://localhost:5229/home");
        }

        try
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogCritical("User not found in system");
                return Redirect("http://localhost:5229/home");
            }

            UserManager = user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Database error: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }

        ViewData["Title"] = $"Editing {what}";
        return Page();
    }

    // post

    private bool IsAuthenticated(string email)
    {
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (claimEmail != email)
        {
            return false;
        }

        return true;
    }
}
