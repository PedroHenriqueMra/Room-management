using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;

[Authorize]
public class RoomModel : PageModel
{
    private readonly ILogger<RoomModel> _logger;
    private readonly DbContextModel _context;
    private readonly IMessageServices _messageService;
    public RoomModel(ILogger<RoomModel> logger, DbContextModel context, IMessageServices messageService)
    {
        _context = context;
        _logger = logger;
        _messageService = messageService;
    }
    public Room Room { get; set; }
    public List<Message> Messages { get; set; }

    public async Task<IActionResult> OnPostAsync(string message, [FromForm] Guid uuid)
    {
        if (uuid == Guid.Empty)
        {
            _logger.LogDebug("The room uuid is empty!!");
            if (!await LoadData(uuid))
            {
                return Page();
            }
            return RedirectToPage("rooms");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError("Null message or empty message");
            if (await LoadData(uuid))
            {
                return Page();
            }
            return RedirectToPage("rooms");
        }
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/auth/login");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (user == null)
        {
            _logger.LogError("User not found!!");
            return RedirectToPage("/auth/login");
        }

        var room = await _context.Room.Include(r => r.Adm).FirstAsync(r => r.Id == uuid);
        Room = room ?? new Room();
        if (room == null)
        {
            _logger.LogError("The room not found!");
            return RedirectToPage("rooms");
        }

        try
        {
            var response = await _messageService.SendMessageAsync(user.Id, room.Id, message);
            if (response is IStatusCodeHttpResult statusCode && statusCode.StatusCode > 299)
            {
                _logger.LogCritical("An error ocurred while the send message");
                if (await LoadData(uuid))
                {
                    return Page();
                }
                return RedirectToPage("rooms");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending message: {ex.Message}");
           if (await LoadData(uuid))
            {
                return Page();
            }
            return RedirectToPage("rooms");
        }

        if (!await LoadData(uuid))
        {
            return RedirectToPage("rooms");
        }

        _logger.LogInformation("Message sent");
        return Page();
    }

    public async Task<IActionResult> OnGetAsync(Guid url)
    {
        if (url == Guid.Empty)
        {
            return RedirectToPage("rooms");
        }
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/home");
        }

        var room = await _context.Room.Include(r => r.Adm).Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == url);
        if (room == null)
        {
            _logger.LogWarning($"the room ({url}) was not found!");
            return RedirectToPage("rooms");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var owner = await _context.User.Include(u => u.Rooms).FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (owner != null)
        {
            if (!room.Users.Any(u => u.Id == owner.Id))
            {
                _logger.LogWarning($"The user ({owner.Name}) isn't in the room!!");
                return RedirectToPage("rooms");
            }
        }
        else
        {
            _logger.LogWarning($"the user ({claimEmail}) was not found!");
            return RedirectToPage("rooms");
        }

        Room = room;
        if (!await LoadData(url))
        {
            return RedirectToPage("rooms");
        }

        return Page();
    }

    private async Task<bool> LoadData(Guid uuid)
    {
        try
        {
            Messages = await _context.Message.Include(m => m.User).Where(r => r.RoomId == uuid).ToListAsync() ?? new List<Message>();
            Room = await _context.Room.Include(r => r.Adm).FirstAsync(r => r.Id == uuid);
        }
        catch
        {
            _logger.LogError("An error ocurred while room data loading");
            return false;
        }

        return true;
    }
}
