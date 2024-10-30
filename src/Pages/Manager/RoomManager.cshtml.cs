using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

[Authorize]
public class RoomManagerModel : PageModel
{
    private readonly ILogger<RoomManagerModel> _logger;
    private readonly DbContextModel _context;

    public RoomManagerModel(ILogger<RoomManagerModel> logger, DbContextModel context)
    {
        _logger = logger;
        _context = context;
    }

    public User currentUserManager;
    public List<Room> userManagedRooms = new List<Room>();
    public List<Room> userIncludedRooms = new List<Room>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"A user with id {id} not found!");
            return Redirect("/home");
        }

        var userClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (user.Email != userClaim)
        {
            _logger.LogWarning($"This id {id} doesn't match the claims email {userClaim}");
            return Redirect("/home");
        }
        currentUserManager = user;

        // rooms that the user is manager
        userManagedRooms = await _context.Room.Where(r => r.AdmId == user.Id).ToListAsync();

        // rooms that the user is included
        userIncludedRooms = await _context.Room.Where(r => r.Users.Any(u => u.Id == user.Id)).ToListAsync();

        return Page();
    }
}
