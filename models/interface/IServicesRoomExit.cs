using ModelTables;

public interface IServicesRoomExit
{
    Task DeleteRoom(DbContextModel context, User adm, Room room);

    Task ExitAllRoomAsync(DbContextModel context, User adm, Room room);

    Task RemoveAdmAsync(DbContextModel context, User adm, Room room);

    Task<object> ExitUserRoomAsync(DbContextModel context, User adm, Room room);
}