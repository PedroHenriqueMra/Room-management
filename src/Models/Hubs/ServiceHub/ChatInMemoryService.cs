using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.DbSet.Models;

namespace MinimalApi.Chat.Services;

public class ChatInMemoryService  : IChatService
{
    private readonly DbContextModel _context;
    private readonly ILogger<ChatInMemoryService> _logger;
    public ChatInMemoryService(DbContextModel context, ILogger<ChatInMemoryService> logger)
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

    public async Task DeleteGroup()
    {

    }
}

public class ChatMessage
{
    public string UserName { get; set; }
    public string Content { get; set; }
}
