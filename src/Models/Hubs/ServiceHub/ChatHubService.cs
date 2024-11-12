using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.DbSet.Models;

namespace MinimalApi.Chat.Services;

public class ChatHubService  : IChatService
{
    private readonly DbContextModel _context;
    private readonly ILogger<ChatHubService> _logger;
    public ChatHubService(DbContextModel context, ILogger<ChatHubService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> CreateGroup(string connectioId, Guid roomId)
    {
        try
        {
            Guid id = Guid.NewGuid();
            var chatRoom = new ChatGroup
            {
                ChatId = id,
                RoomId = roomId
            };
            chatRoom.ConnectionIds.Add(connectioId);

            await _context.ChatGroup.AddAsync(chatRoom);
            await _context.SaveChangesAsync();

            return id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning($"Error in update database. Message: {ex}");
            return Guid.Empty;
        }
    }

    public async Task DeleteGroup(Guid roomId)
    {
        var chatGroup = await GetChatGroupAsync(roomId);
        if (chatGroup != null)
        {
            _context.Entry(chatGroup).State = EntityState.Deleted;
            _context.ChatGroup.Remove(chatGroup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncludeClient(string connectionId, Guid roomId)
    {
        var chatGroup = await GetChatGroupAsync(roomId);
        if (chatGroup == null)
        {
            _logger.LogWarning($"The room {roomId} don't exists");
            return;
        }

        if (!chatGroup.ConnectionIds.Contains(connectionId))
        {
            chatGroup.ConnectionIds.Add(connectionId);
            await _context.SaveChangesAsync();
            return;
        }

        _logger.LogInformation($"The user {connectionId} already exists in room!");
    }

    public async Task RemoveClient(string connectionId, Guid roomId)
    {
        var chatGroup = await GetChatGroupAsync(roomId);
        if (chatGroup == null)
        {
            _logger.LogWarning($"The room {roomId} don't exists");
            return;
        }

        if (chatGroup.ConnectionIds.Contains(connectionId))
        {
            chatGroup.ConnectionIds.Remove(connectionId);
            await _context.SaveChangesAsync();
            return;
        }

        _logger.LogInformation($"The user {connectionId} already is no longer this room");
    }

    private async Task<ChatGroup> GetChatGroupAsync(Guid roomId)
    {   
        return await _context.ChatGroup.FirstOrDefaultAsync(c => c.RoomId == roomId);
    }
}
