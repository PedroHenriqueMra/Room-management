using System.Text.RegularExpressions;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.Chat.Services;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;

namespace MinimalApi.Chat;
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly DbContextModel _context;
    private readonly ILogger<ChatHub> _logger;
    private readonly HttpContext _httpContext;
    public ChatHub(IChatService chatService, DbContextModel context, ILogger<ChatHub> logger, IHttpContextAccessor httpContext)
    {
        _chatService = chatService;
        _context = context;
        if (httpContext.HttpContext != null)
        {
            _httpContext = httpContext.HttpContext;
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        var roomId = GetRoomId();
        if (roomId == Guid.Empty)
        {
            _logger.LogError("The room id is null!");
            
            await Clients.Caller.SendAsync("ReceiveError", "error-onnection", "await we are fixing it...");
            
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var roomId = GetRoomId();

        await _chatService.RemoveClient(Context.ConnectionId, roomId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddToGroup(Guid roomId)
    {
        var queryChat = await _context.ChatGroup.FirstOrDefaultAsync(c => c.RoomId == roomId);
        if (queryChat != null)
        {
            await _chatService.IncludeClient(Context.ConnectionId, roomId);
        }
        else
        {
            await _chatService.CreateGroup(Context.ConnectionId, roomId);
        }
    }

    // adm function:
    // public async Task RemoveFromGroup(Guid roomId)
    // {

    // }

    public async Task SendMesageToGroup(int userId, string message, Guid roomId)
    {
        var room = await GetChatAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room not found");
            await Clients.Caller.SendAsync("ReceiveMessage", "Error", "room not found");
            return;
        }

        var user = await _context.User.FindAsync(userId);
        if (user != null)
        {
            var userInfos = new {
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = user.Name
            };
            await Clients.Clients(room.ConnectionIds)
                .SendAsync("ReceiveMessage", JsonConvert.SerializeObject(userInfos), message, roomId);
        }
        else
        {
            _logger.LogWarning($"The user wich id {userId} didm't matche");
            await Clients.Caller.SendAsync("ReceiveError", "error-onnection", "await we are fixing it...");
        }
    }

    private async Task<ChatGroup> GetChatAsync(Guid roomId)
    {
        return await _context.ChatGroup.FirstOrDefaultAsync(c => c.RoomId == roomId);
    }

    private Guid GetRoomId()
    {
        // queryString: ?chatId=.....
        var queryString = _httpContext.Request.Query["chatId"].ToString();
        if (Guid.TryParse(queryString, out Guid id))
        {
            return id;
        }

        return Guid.Empty;
    }
}
