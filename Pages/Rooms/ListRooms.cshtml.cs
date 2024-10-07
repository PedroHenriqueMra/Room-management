using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;

[Authorize]
public class ListRoomModel : PageModel
{
    public ILogger<ListRoomModel> _logger;
    public DbContextModel _context;
    public HttpClient _httpClient;

    public ListRoomModel(ILogger<ListRoomModel> logger, DbContextModel context, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public List<Room> Rooms { get; set; } = new List<Room>();
    public User Owner { get; set; } = new User();

    public async Task<IActionResult> OnPostAsync([FromForm] bool isPrivate, [FromForm] Guid uuid)
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("");
        }

        Owner = await GetAuthenticatedUserAsync();
        if (Owner == null)
        {
            _logger.LogError($"User {Owner} is null");
            return NotFound("User not found");
        }

        // dados par a requisição
        var room = await _context.Room.FirstOrDefaultAsync(r => r.Id == uuid);
        if (room == null)
        {
            _logger.LogError($"Room {room} is null");
            return NotFound("Room not found");
        }

        if (room.UsersNames.Contains(Owner.Name))
        {
            _logger.LogInformation($"The user {Owner.Name} already existes in this room!");
            return Page();
        }

        var data = new
        {
            UserParticipe = Owner,
            Room = room
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // envio de dados para a api
        try
        {
            UriBuilder url = new UriBuilder("http://localhost:5229/");
            url.Path = room.IsPrivate ? $"/room/request/{uuid}" : $"/room/participate/{uuid}";

            var response = await _httpClient.PostAsync(url.ToString(), content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error in httpRequest. Status-Code: {response.StatusCode}");
                return RedirectToAction("");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Code exception: {ex}");
            return (IActionResult)Results.BadRequest();
        }

        return RedirectToAction("");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogError("User isn't authenticated!");
            return RedirectToAction("");
        }

        Owner = await GetAuthenticatedUserAsync();
        var rooms = await _context.Room.Include(r => r.Adm).ToListAsync();
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
        return await _context.User.FirstOrDefaultAsync(u => u.Email == claimEmail);
    }
}
