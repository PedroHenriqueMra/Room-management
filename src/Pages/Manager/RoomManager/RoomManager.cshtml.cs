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
    private readonly IExitRoomService _exitRoomService;

    public RoomManagerModel(ILogger<RoomManagerModel> logger, DbContextModel context, IExitRoomService exitRoomService)
    {
        _logger = logger;
        _context = context;
        _exitRoomService = exitRoomService;
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

        // load all data:
        await LoadDatas(user);

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid roomId)
    {
        if (roomId == Guid.Empty)
        {
            _logger.LogWarning("This room not exist");
            return Page();
        }

        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("User don't authenticated");
            return Redirect("/auth/login");
        } 

        var idClaims = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await AuthUserAsync(idClaims);
        if (user == null)
        {
            _logger.LogWarning("User not found");
            return Redirect("/auth/login");
        }
        
        // load all data:
        await LoadDatas(user);
        
        var room = await _context.Room.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == roomId);
        if (room.AdmId != idClaims)
        {
            _logger.LogWarning("You aren't the admin of this room");
            return Page();
        }

        try
        {
            await _exitRoomService.DeleteRoom(user, room);
            _logger.LogInformation("Room deleted");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"An error ocurred while deleted the room {room.Name}. Message: {ex}");
            return Page();
        }
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

    private async Task LoadDatas(User user)
    {
        // rooms that the user is manager
        userManagedRooms = await _context.Room.Where(r => r.AdmId == user.Id).ToListAsync();

        // rooms that the user is included
        userIncludedRooms = await _context.Room.Where(r => r.Users.Any(u => u.Id == user.Id)).ToListAsync();
    }
}
