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
    private readonly HttpClient _httpClient;
    public Room Room { get; set; }
    public List<Message> Messages { get; set; }

    public RoomModel(ILogger<RoomModel> logger, DbContextModel context, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> OnPostAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError("Null message or empty message");
            return Page();
        }
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("/login");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (user == null)
        {
            _logger.LogError("User not found!!");
            return RedirectToAction("/login");
        }

        if (message.Length < 1 || message.Length > 200)
        {
            _logger.LogWarning("The message must contain a length of: (0 => 200) characters");
            return Page();
        }

        var path = HttpContext.Request.Path.Value;
        var url = path.Split("/room/")[1];
        if (!Guid.TryParse(url, out var roomId))
        {
            return NotFound("Page not found!!");
        }

        var room = await _context.Room.Include(r => r.Adm).FirstAsync(r => r.Id == roomId);
        Room = room ?? new Room();
        if (room == null)
        {
            _logger.LogError("The room not found!");
            return Page();
        }

        var data = new
        {
            Content = message,
            IdRoom = roomId,
            IdUser = user.Id,
        };
        var json = JsonConvert.SerializeObject(data);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        Uri GetUri(string path) => new Uri("http://localhost:5229" + path);
        try
        {
            var response = await _httpClient.PostAsync(GetUri("/new/message"), httpContent);
            if (!response.IsSuccessStatusCode)
            {
                Messages = await _context.Message.Include(m => m.User).Where(r => r.RoomId == roomId).ToListAsync() ?? new List<Message>();
                _logger.LogError("Error sending message!!");
                return Page();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Error sending message: {ex.Message}");
            return Page();
        }

        Messages = await _context.Message.Include(m => m.User).Where(r => r.RoomId == roomId).ToListAsync() ?? new List<Message>();
        _logger.LogInformation("Message sent");
        return Page();
    }

    public async Task<IActionResult> OnGetAsync(Guid url)
    {
        if (url == Guid.Empty)
        {
            return RedirectToAction("/rooms");
        }
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("");
        }

        var room = await _context.Room.Include(r => r.Adm).FirstOrDefaultAsync(r => r.Id == url);
        if (room == null)
        {
            _logger.LogWarning($"the room ({url}) was not found!");
            return RedirectToAction("/rooms");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var owner = await _context.User.FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (owner != null)
        {
            if (!room.UsersNames.Contains(owner.Name))
            {
                _logger.LogWarning($"The user ({owner.Name}) isn't in the room!!");
                return RedirectToAction("/rooms");
            }
        }
        else
        {
            _logger.LogWarning($"the user ({claimEmail}) was not found!");
            return RedirectToAction("/rooms");
        }

        Room = room;
        Messages = await _context.Message.Include(m => m.User).Where(r => r.RoomId == url).ToListAsync();

        return Page();
    }
}
