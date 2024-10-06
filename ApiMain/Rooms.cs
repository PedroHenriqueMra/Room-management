using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ModelTables;

namespace MinimalApi.Endpoints.ConfigureRooms;
public static class Rooms
{
    public static void EndpointsRooms(this WebApplication app)
    {
        // criar uma nova sala
        app.MapPost("/new/room", async (DbContextModel context, CreateRoomRequest roomData) =>
        {
            var adm = await context.User.FindAsync(roomData.IdUser);
            if (adm == null)
            {
                return Results.NotFound("Usuário não encontrado!");
            }

            var existingRoom = await context.Room.FirstOrDefaultAsync(r => r.Name == roomData.Name);
            if (existingRoom != null)
            {
                return Results.BadRequest("Já existe uma sala com este nome!!");
            }

            var newRoom = new Room()
            {
                Name = roomData.Name,
                Adm = adm,
                Description = roomData.Description,
                AdmId = adm.Id,
                IsPrivate = roomData.IsPrivate
            };
            newRoom.Users.Add(adm);
            newRoom.UsersNames.Add(adm.Name);
            adm.Rooms.Add(newRoom);
            adm.RoomsNames.Add(newRoom.Name);

            await context.AddAsync(newRoom);
            await context.SaveChangesAsync();

            return Results.Ok("Sala criada com êxito!!");
        });

        // deletar uma sala (apenas adm)
        app.MapDelete("/room/delete/{uuid:guid}/{id:int}", [Authorize] async (DbContextModel context, Guid uuid, int id) =>
        {
            var room = await context.Room.FindAsync(uuid);
            if (room == null)
            {
                return Results.NotFound("Sala não encontrada!");
            }

            var adm = await context.User.FindAsync(id);
            if (adm == null)
            {
                return Results.NotFound("Usuário não encontardo!!");
            }

            if (room.AdmId != adm.Id)
            {
                return Results.BadRequest("O usuário não contem as permissões necessárias para esta ação!!");
            }

            var servicesRoom = new ServicesRoomDelete();
            servicesRoom.DeleteRoom(context, adm, room);

            return Results.Ok("Sala deletada com êxito!!");
        });

        // participar de uma sala (sala publica)
        app.MapPost("/room/participate/{uuid:guid}", async (DbContextModel context, ParticipeRoomData roomData) =>
        {
            Console.WriteLine("testeee: sala publica");
            return Results.StatusCode(200);
        });

        // solicitar entrada na sala (sala privada)
        app.MapPost("/room/request/{uuid:guid}", async (DbContextModel context, ParticipeRoomData roomData) =>
        {
            Console.WriteLine("testeee: sala privada");
            return Results.StatusCode(200);
        });

        // sair de uma sala
        app.MapPut("/room/exit/", async (DbContextModel context) =>
        {
           
        });

    }
}

