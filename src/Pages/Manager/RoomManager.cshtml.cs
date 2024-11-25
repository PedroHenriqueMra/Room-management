using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using NuGet.Protocol;

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

    [BindProperty]
    public Guid RoomId { get; set; }
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

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        var user = await AuthUserAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"An error ocurred while the authentication for {id} ocorred");
            return Redirect("/home");
        }

        if (RoomId == Guid.Empty)
        {
            _logger.LogWarning("The room id is empty!. probably an internal error");
            return Redirect($"/user/manager/room/{id}");
        }

        var room = await _context.Room.FindAsync(RoomId);
        if (room == null)
        {
            _logger.LogWarning($"The room searched {RoomId} wasn't found!");
            return Redirect($"/home");
        }

        if (room.AdmId != user.Id)
        {
            _logger.LogWarning($"The user {id} wasn't owner of the room {room.Name}!");
            return Redirect($"/home");
        }

        var filePath = "~/Pages/Manager/Partials/_ManageRoomPartial.cshtml";
        return Partial(filePath, this);
    }
    private async Task<User> AuthUserAsync(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"User {id} not found");
            return null;
        }
        var userClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userClaim != user.Email)
        {
            _logger.LogWarning($"The email claim don't match with the user found!. Email calim: {userClaim}. Email user: {user.Email}");
            return null;
        }

        return user;
    }
}
