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
    public int countManageRooms;

    [Authorize]
    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id == null)
        {
            _logger.LogWarning("The id is null");
            return Redirect("/home");
        }
        
        var user = await _context.User.FindAsync(id);
        countManageRooms = await _context.Room.Where(r => r.AdmId == user.Id).CountAsync();

        if (user == null)
        {
            _logger.LogWarning($"None users with id {id} was found");
            return Redirect("/home");
        }
        var userClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userClaim != null)
        {
            if (userClaim != user.Email)
            {
                _logger.LogError($"Error: Url doesn'n match the user whose claims email is {userClaim}");
                return Redirect("/home");
            }
            _logger.LogInformation("You have entered the user manager page!!");
        }
        else
        {
            _logger.LogError("Error: claims email not found");
            return NotFound("You aren't logged into your account!!.");
        }

        UserManager = user;
        return Page();
    }
}
