using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders.Testing;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;

[IgnoreAntiforgeryToken]
// [Authorize]
public class RoomModel : PageModel
{
    private readonly ILogger<RoomModel> _logger;
    private readonly DbContextModel _context;
    private readonly IMessageServices _messageService;
    private readonly IGetPropertyAnonymous _getPropAnonymous;
    public RoomModel(ILogger<RoomModel> logger, DbContextModel context, IMessageServices messageService, IGetPropertyAnonymous getMessageError)
    {
        _context = context;
        _logger = logger;
        _messageService = messageService;
        _getPropAnonymous = getMessageError;
    }
    public Room Room { get; set; }
    public List<Message> Messages { get; set; }

    public class DataForCreateMessage
    {
        public int? UserId { get; set; }
        public Guid? RoomId { get; set; }
        public string? Message { get; set; }
    }
    // endpoint to fetch js
    public async Task<IActionResult> OnPostAsync([FromBody]DataForCreateMessage? RequestData)
    {
        string content;
        if (RequestData?.UserId == null || RequestData?.RoomId == null || RequestData?.Message == null)
        {
            _logger.LogWarning("Algum dado (userId, RoomId, Message) nao foi preenchido no cliente.");
            content = "Algo deu errado!. Dados nececssarios nao preenchidos";

            return StatusCode(400, JsonConvert.SerializeObject(content));
        }
        User user = await GetUserWithAuthentication(RequestData.UserId);
        if (user == null)
        {
            _logger.LogWarning($"User not found. The user {RequestData.UserId} don't matche with email claims");
            content = "Algo deu errado!. Erro de autenticação";

            return StatusCode(401, JsonConvert.SerializeObject(content));
        }

        // create message with service
        var request = await _messageService.CreateMessageAsync(RequestData.UserId, RequestData.RoomId, RequestData.Message);

        if (request is IStatusCodeHttpResult status && status.StatusCode > 299)
        {
            content = _getPropAnonymous.GetMessage(request, "Value", '=', '}');
            _logger.LogWarning($"An error ocurred while create a new message.\nError message: {content}");

            return StatusCode(400, JsonConvert.SerializeObject(content));
        }

        content = _getPropAnonymous.GetMessage(request, "ResponseContent",null,null);
        return StatusCode(200, content);
    }

    public async Task<IActionResult> OnGetAsync(Guid url)
    {
        if (url == Guid.Empty)
        {
            return Redirect("/rooms");
        }
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/home");
        }

        var room = await _context.Room.Include(r => r.Adm).Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == url);
        if (room == null)
        {
            _logger.LogWarning($"the room ({url}) was not found!");
            return Redirect("/rooms");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var owner = await _context.User.Include(u => u.Rooms).FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (owner != null)
        {
            if (!room.Users.Any(u => u.Id == owner.Id))
            {
                _logger.LogWarning($"The user ({owner.Name}) isn't in the room!!");
                return Redirect("/rooms");
            }
        }
        else
        {
            _logger.LogWarning($"the user ({claimEmail}) was not found!");
            return Redirect("/rooms");
        }

        Room = room;
        if (!await LoadData(url))
        {
            return Redirect("/rooms");
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

    private async Task<User> GetUserWithAuthentication(int? id)
    {
        var claims = User.FindFirst(ClaimTypes.Email)?.Value;
        if (claims == null)
        {
            return null;
        }

        var findUser = await _context.User.FindAsync(id);
        if (findUser == null)
        {
            return null;
        }

        if (findUser.Email != claims)
        {
            return null;
        }

        return findUser;
    }
}
