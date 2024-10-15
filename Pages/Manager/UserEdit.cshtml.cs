using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using MySqlX.XDevAPI.Common;
using NuGet.Protocol;
using Services.ServicesUser.Change;

[Authorize]
public class UserEditModel : PageModel
{
    private readonly ILogger<UserEditModel> _logger;
    private readonly DbContextModel _context;
    private readonly IUserManageServices _serviceChange;

    public UserEditModel(ILogger<UserEditModel> logger, DbContextModel context, IUserManageServices serviceChange)
    {
        _logger = logger;
        _context = context;
        _serviceChange = serviceChange;
    }

    [BindProperty]
    public UserChangeData Input { get; set; }
    public User UserManager { get; set; }
    public string What { get; set; }
    private static readonly List<string> AllowedFields = new List<string> { "name", "email", "password" };
    
    // post
    [ValidateAntiForgeryToken()]
    public async Task<IActionResult> OnPostAsync([FromForm] string? type, [FromForm] string? email)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Error data");
            return RedirectToPage($"/user/edit/{type}/{email}");
        }
        var validType = IsValidFieldType(type);

        if (!validType)
        {
            _logger.LogWarning("Request type not informed or invalid!");
            return RedirectToPage($"/user/edit/{type}/{email}");
        }
        email = String.IsNullOrEmpty(email) ? null : email ;
        if (email == null)
        {
            _logger.LogWarning("User email error, This email is null or empty");
            return RedirectToPage("/auth/login");
        }
        if (!IsAuthenticated(email))
        {
            _logger.LogWarning("Unauthenticated user");
            return RedirectToPage("/auth/login");
        }
        
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        if (type == "password")
        {
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password);
        }
        else
        {
            var result = await _serviceChange.ChangeWithTypeAsync(_logger, _context, user, Input, type);

            if (result is IStatusCodeHttpResult status && status.StatusCode > 299)
            {
                _logger.LogCritical($"A response BadRequest was received");
                return RedirectToPage($"/user/edit/{type}/{email}");
            }
            else if (result is Conflict)
            {
                _logger.LogCritical($"A response Conflict was received");
                return RedirectToPage($"/user/edit/{type}/{email}");
            }
        }
        
         
        return Redirect($"/user/manager/{user.Email}");
    }

    public async Task<IActionResult> OnGetAsync(string what, string email)
    {
        if (String.IsNullOrEmpty(what) || String.IsNullOrEmpty(email))
        {
            _logger.LogCritical("Url data is empty");
            return RedirectToPage("/home");
        }

        if (!AllowedFields.Contains(what))
        {
            _logger.LogCritical("Url isn't allowed");
            return RedirectToPage("/home");
        }

        if (!IsAuthenticated(email))
        {
            _logger.LogCritical("User isn't authenticate");
            return RedirectToPage("/home");
        }

        try
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogCritical("User not found in system");
                return RedirectToPage("/home");
            }

            UserManager = user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Database error: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }

        What = what;
        ViewData["Email"] = email;
        ViewData["Title"] = $"Editing {what}";
        return Page();
    }

    private bool IsValidFieldType(string type) => AllowedFields.Contains(type);

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
