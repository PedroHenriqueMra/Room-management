using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

namespace Services.RoomServices.Create;

public class ServicesCreateRoom : IServicesCreateRoom
{
    private readonly DbContextModel _context;

    public ServicesCreateRoom(DbContextModel context)
    {
        _context = context;
    }

    public async Task<IResult> CreateRoomAsync(CreateRoomRequest roomData)
    {
        var adm = await _context.User.FindAsync(roomData.IdUser);
        if (adm == null)
        {
            return Results.NotFound("Usuário não encontrado!");
        }

        var existingRoom = await _context.Room.FirstOrDefaultAsync(r => r.Name == roomData.Name);
        if (existingRoom != null)
        {
            return Results.BadRequest("Já existe uma sala com este nome!!");
        }

        try
        {
            var newRoom = new Room()
            {
                Name = roomData.Name,
                Adm = adm,
                Description = roomData.Description,
                AdmId = adm.Id,
                IsPrivate = roomData.IsPrivate
            };
            newRoom.Users.Add(adm);
            newRoom.UserName.Add(adm.Name);
            adm.Rooms.Add(newRoom);
            adm.RoomsNames.Add(newRoom.Name);

            await _context.AddAsync(newRoom);
            await _context.SaveChangesAsync();

            return Results.Ok("Sala criada com êxito!!");
        }
        catch (DbUpdateException ex)
        {
            return Results.BadRequest(new { Error = "An error ocurred in save room in database" });
        }
    }
}
