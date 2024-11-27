using MinimalApi.DbSet.Models;

public interface IExitRoomService
{
    Task DeleteRoom(User adm, Room room);

    Task<object> ExitUserRoomAsync(User adm, Room room);
}