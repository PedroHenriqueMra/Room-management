using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Digests;
using Services.ServicesRoom.Enter;

[Authorize]
public class RoomsListModel : PageModel
{
    private readonly ILogger<RoomsListModel> _logger;
    private readonly DbContextModel _context;
    private readonly IServicesEnterRoom _serviceRoom;

    public RoomsListModel(ILogger<RoomsListModel> logger, DbContextModel context, IServicesEnterRoom serviceRoom)
    {
        _context = context;
        _logger = logger;
        _serviceRoom = serviceRoom;
    }
    public List<Room> Rooms { get; set; } = new List<Room>();
    public User Owner { get; set; } = new User();

    public async Task<IActionResult> OnPostAsync([FromForm] Guid uuid, [FromForm] string? password)
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/home");
        }

        Owner = await GetAuthenticatedUserAsync();
        if (Owner == null)
        {
            _logger.LogError($"User {Owner} is null");
            return NotFound("User not found");
        }

        var room = await _context.Room.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == uuid);
        if (room == null)
        {
            _logger.LogError($"Room {room} is null. It doesn't exist.");
            return NotFound("Room not found");
        }

        try
        {
            bool alreadyContains = await _context.Room
                        .Include(r => r.Users)
                        .Where(r => r.Id == uuid)
                        .AnyAsync(r => r.Users.Any(u => u.Id == Owner.Id));

            if (alreadyContains)
            {
                _logger.LogInformation($"The user {Owner.Name} already existes in this room!");
                return RedirectToPage($"/rooms/{uuid}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while checking user existence: {ex.Message}");
            return Page();
        }

        var data = new ParticipeRoomData()
        {
            User = Owner,
            Room = room,
            Password = room.IsPrivate ? password : null
        };
        try
        {
            var response = await _serviceRoom.IncludeUserAsync(_logger, _context, data);

            if (response is IStatusCodeHttpResult status && status.StatusCode > 299)
            {
                _logger.LogError($"Error when Entering the room. Room: {room.Name}");
                return RedirectToPage("/rooms");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Code exception: {ex}");
            return (IActionResult)Results.BadRequest();
        }

        return Redirect($"http://localhost:5229/rooms/{uuid}");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogError("User isn't authenticated!");
            return RedirectToPage("/home");
        }

        Owner = await GetAuthenticatedUserAsync();
        var rooms = await _context.Room.AsNoTracking().Include(r => r.Adm).Include(r => r.Users).ToListAsync();
        if (rooms.Count != 0)
        {
            foreach (var r in rooms)
            {
                Rooms.Add(r);
            }
        }

        return Page();
    }

    private async Task<User> GetAuthenticatedUserAsync()
    {
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return await _context.User.Include(u => u.Rooms).FirstOrDefaultAsync(u => u.Email == claimEmail);
    }
}
