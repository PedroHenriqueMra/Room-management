using MinimalApi.DbSet.Models;
using MinimalApi.DbSet.Models;

namespace Services.ServicesRoom.Delete;

public class ServicesExitRoom : IServicesExitRoom
{
    public async Task DeleteRoom(DbContextModel context, User adm, Room room)
    {
        await ExitAllRoomAsync(context, adm, room);

        context.Remove(room);
    }

    public async Task ExitAllRoomAsync(DbContextModel context, User adm, Room room)
    {
        var usersInRoom = room.Users.ToList();
        foreach(var user in usersInRoom)
        {
            user.Rooms.Remove(room);
            user.RoomsNames.Remove(room.Name);
        }

        await RemoveAdmAsync(context, adm, room);
    }

    public async Task RemoveAdmAsync(DbContextModel context, User adm, Room room)
    {
        adm.Rooms.Remove(room);
        adm.RoomsNames.Remove(room.Name);

        await context.SaveChangesAsync();
    }

    public async Task<object> ExitUserRoomAsync(DbContextModel context, User user, Room room)
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
