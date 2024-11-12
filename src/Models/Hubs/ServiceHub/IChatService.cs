using MinimalApi.DbSet.Models;

namespace MinimalApi.Chat.Services;

public interface IChatService
{
    Task<Guid> CreateGroup(string connectioId, Guid roomId);
    Task DeleteGroup(Guid roomId);
    Task IncludeClient(string connectionId, Guid roomId);
    Task RemoveClient(string connectionId, Guid roomId);
}
