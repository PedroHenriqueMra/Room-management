using MinimalApi.DbSet.Models;
using MinimalApi.DbSet.Models;

namespace Services.ServicesRoom.Delete;

public class ExitRoomService : IExitRoomService
{
    private readonly DbContextModel _context;
    public ExitRoomService(DbContextModel context)
    {
        _context = context;
    }

    public async Task DeleteRoom(User adm, Room room)
    {
        if (room.AdmId != adm.Id)
        {
            return;
        }

        await ExitAllOfRoomAsync(room);

        _context.Attach(room);
        _context.Remove(room);
        await _context.SaveChangesAsync();
    }

    private async Task ExitAllOfRoomAsync(Room room)
    {
        var usersInRoom = room.Users.ToList();
        foreach(var user in usersInRoom)
        {
            user.Rooms.Remove(room);
            user.RoomsNames.Remove(room.Name);
        }
    }

    public async Task<object> ExitUserRoomAsync(User user, Room room)
    {
        if (user.Id == room.AdmId)
        {
            return Results.BadRequest("Você tem um cargo de adm nesta sala!! para sair remova a sala.");
        }

        user.Rooms.Remove(room);
        user.RoomsNames.Remove(room.Name);

        room.Users.Remove(user);
        room.UserName.Remove(user.Name);

        return Results.Ok("Você não faz mais parte da sala!!");
    }
}
