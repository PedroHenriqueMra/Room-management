using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ModelTables;
using Newtonsoft.Json;

[Authorize]
public class RoomModel : PageModel
{
    private readonly ILogger<RoomModel> _logger;
    private readonly DbContextModel _context;
    public Room Room { get; set; }
    public User Adm { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();

    public RoomModel(ILogger<RoomModel> logger, DbContextModel context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync(string message)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user == null)
        {
            _logger.LogError("User not found!!");
            return Page();
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

        var room = await _context.Room.FindAsync(roomId);
        if (room == null)
        {
            _logger.LogError("The room not found!");
            return Page();
        }

        var data = new
        {
            Content = message,
            IdRoom = roomId,
            IdUser = user.Id
        };
        var json = JsonConvert.SerializeObject(data);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        Uri GetUri(string path) => new Uri("http://localhost:5229" + path);
        using (var handler = new HttpClientHandler())
        using (var client = new HttpClient(handler))
        {
            try
            {          
                var response = await client.PostAsync(GetUri("/new/message"), httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error sending message!!");
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return Page();
            }

            return Page();
        }
    }

    public async Task<IActionResult> OnGetAsync(Guid url)
    {
        if (url == Guid.Empty)
        {
            return RedirectToAction("/rooms");
        }

        var room = await _context.Room.Include(r => r.Adm).FirstOrDefaultAsync(r => r.Id == url);
        if (room == null)
        {
            _logger.LogWarning($"the room ({url}) was not found!");
            return RedirectToAction("/rooms");
        }

        var owner = await _context.User.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (owner != null)
        {
            if (!room.UsersNames.Contains(owner.Name))
            {
                _logger.LogWarning($"The user ({owner.Name}) isn´t in the room!!");
                return RedirectToAction("/rooms");
            }
        }
        else
        {
            _logger.LogWarning($"the user ({User.Identity?.Name}) was not found!");
            return RedirectToAction("/rooms");
        }

        Room = room;
        Messages = room.Messages.ToList();
        Adm = room.Adm;

        return Page();
    }
}
