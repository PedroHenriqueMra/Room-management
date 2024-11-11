using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.Chat.Services;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Asn1;

namespace MinimalApi.Chat;
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly DbContextModel _context;
    private readonly ILogger<ChatHub> _logger;
    public ChatHub(IChatService chatService, DbContextModel context, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _context = context;
    }
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddToGroup(Guid roomId)
    {
        var chatRoom = await GetChatAsync(roomId);
        if (chatRoom == null)
        {
            await _chatService.CreateGroup(Context.ConnectionId, roomId);
        }

        if (!chatRoom.ConnectionIds.Contains(Context.ConnectionId))
        {
            chatRoom.ConnectionIds.Add(Context.ConnectionId);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveFromGroup(Guid roomId)
    {
        var chatRoom = await GetChatAsync(roomId);
        if (chatRoom == null) 
        {
            _logger.LogWarning($"The room {roomId} not found");
            await Clients.Caller.SendAsync("ReceiveMessage", "error", "room not found");
            return;
        }

        var isInGroup = chatRoom.ConnectionIds.Contains(Context.ConnectionId);
        if (isInGroup)
        {
            try
            {
                chatRoom.ConnectionIds.Remove(Context.ConnectionId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"The user {Context.ConnectionId} isn't in the room: {roomId}");
            }
            
            return;
        }

        _logger.LogWarning($"User {Context.ConnectionId} not found in this room {roomId}");
        await Clients.Caller.SendAsync("ReceiveMessage", "error", "user not found");
    }

    public async Task SendMesageToGroup(string name, string message, Guid roomId)
    {
        var room = await GetChatAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room not found");
            await Clients.Caller.SendAsync("ReceiveMessage", "error", "room not found");
            return;
        }

        if (room.ConnectionIds.Count > 0)
        {
            var userMessage = new ChatMessage {
            UserName = name,
            Content = message
            };
            await Clients.Clients(room.ConnectionIds)
                .SendAsync("ReceiveMessage", name, message);
        }
    }

    private async Task<ChatGroup> GetChatAsync(Guid roomId)
    {
        return await _context.ChatGroup.FirstOrDefaultAsync(c => c.RoomId == roomId);
    }
}
