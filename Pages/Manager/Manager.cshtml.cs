using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using ModelTables;
using System.Security.Claims;

[Authorize]
public class ManagerModel : PageModel
{
    private readonly ILogger<ManagerModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly DbContextModel _context;
    public ManagerModel(DbContextModel context, ILogger<ManagerModel> logger, UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public User User { get; set; }

    public async Task<IActionResult> OnGetAsync(string email)
    {
        if (String.IsNullOrEmpty(email))
        {
            return RedirectToPage("/");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"Login with a valid user. Email: {email}");
        }

        var query = await _context.User.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (query == null)
        {
            return NotFound($"Imposible login for {email}");
        }

        User = query;
        return Page();
    }
}
