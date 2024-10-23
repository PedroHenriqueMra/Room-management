using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.DbSet.Models;
using System.Security.Claims;

[Authorize]
public class ManagerModel : PageModel
{
    private readonly ILogger<ManagerModel> _logger;
    private readonly DbContextModel _context;
    public ManagerModel(DbContextModel context,ILogger<ManagerModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public User UserManager { get; set; }

    [Authorize]
    public async Task<IActionResult> OnGetAsync(string email)
    {
        if (String.IsNullOrEmpty(email))
        {
            return Redirect("http://localhost:5229/home");
        }

        var userClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userClaim != null)
        {
            if (userClaim != email)
            {
                _logger.LogError($"Error: Url doesn'n match the user whose claims email is {userClaim}");
                return NotFound("You aren't logged into your account!!.");
            }
            _logger.LogInformation("You have entered the user manager page!!");
        }
        else
        {
            _logger.LogError("Error: claims email not found");
            return NotFound("You aren't logged into your account!!.");
        }

        var query = await _context.User.FirstOrDefaultAsync(u => u.Email == userClaim);
        if (query == null)
        {
            _logger.LogError($"User for email: {email} not found in DbContext!!");
            return NotFound($"Error: unable to find login for {email}.");
        }

        UserManager = query;
        return Page();
    }
}
