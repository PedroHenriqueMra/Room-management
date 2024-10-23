using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using MinimalApi.Services.Utils.RegularExpression.Room;

namespace Services.RoomServices.Create;

public class RoomCreateService : IRoomCreateService
{
    private readonly DbContextModel _context;

    public RoomCreateService(DbContextModel context)
    {
        _context = context;
    }

    public async Task<IResult> CreateRoomAsync(CreateRoomRequest roomData)
    {
        if (roomData.IsPrivate)
        {
            if (roomData.Password == null)
            {
                return Results.BadRequest("A password is required for this room");
            }
            
            bool isValid = RoomCheckRegularExpression.RoomPasswordIsValid(roomData.Password);
            if (!isValid)
            {
                return Results.BadRequest($"Error, invalid characters for this password {roomData.Password}");
            }
        }

        var adm = await _context.User.Include(u => u.Rooms).FirstAsync(u => u.Id == roomData.IdAdm);
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
                IsPrivate = roomData.IsPrivate,
                Password = roomData.Password ?? null
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
