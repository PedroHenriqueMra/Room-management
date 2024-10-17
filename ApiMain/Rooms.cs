using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Services.ServicesRoom.Delete;
using Services.ServicesRoom.Enter;

namespace MinimalApi.Endpoints.ConfigureRooms;
public static class Rooms
{
    public static void EndpointsRooms(this WebApplication app)
    {
        // criar uma nova sala
        // app.MapPost("/new/room", async (DbContextModel context, CreateRoomRequest roomData) =>
        // {
        //     var adm = await context.User.FindAsync(roomData.IdUser);
        //     if (adm == null)
        //     {
        //         return Results.NotFound("Usuário não encontrado!");
        //     }

        //     var existingRoom = await context.Room.FirstOrDefaultAsync(r => r.Name == roomData.Name);
        //     if (existingRoom != null)
        //     {
        //         return Results.BadRequest("Já existe uma sala com este nome!!");
        //     }

        //     var newRoom = new Room()
        //     {
        //         Name = roomData.Name,
        //         Adm = adm,
        //         Description = roomData.Description,
        //         AdmId = adm.Id,
        //         IsPrivate = roomData.IsPrivate
        //     };
        //     newRoom.Users.Add(adm);
        //     newRoom.UserName.Add(adm.Name);
        //     adm.Rooms.Add(newRoom);
        //     adm.RoomsNames.Add(newRoom.Name);

        //     await context.AddAsync(newRoom);
        //     await context.SaveChangesAsync();

        //     return Results.Ok("Sala criada com êxito!!");
        // });

        // deletar uma sala (apenas adm)

        // participar de uma sala (sala publica)
        // app.MapPost("/room/participate/{uuid:guid}", async (ILogger<Program> log, DbContextModel context, [FromBody] ParticipeRoomData roomData) =>
        // {
        //     bool dataEmpty = roomData.Room == null || roomData.UserParticipe == null ? true : false;

        //     if (dataEmpty)
        //     {
        //         Console.WriteLine("Request with empty data!");
        //         return Results.StatusCode(204); // empty data
        //     }

        //     var entityRoom = await context.Room.Include(r => r.Users).FirstAsync(r => r.Id == roomData.Room.Id);
        //     var entityUser = await context.User.Include(u => u.Rooms).FirstAsync(u => u.Id == roomData.UserParticipe.Id);

        //     var serviceRoom = new ServicesEnterRoom();
        //     var resultService = await serviceRoom.IncludeUserAsync(log, context, entityUser, entityRoom);

        //     return resultService;
        // });


        //deletar todas as salas
        app.MapDelete("/room/delete", async (DbContextModel context) =>
        {
            var room = await context.Room.Include(r => r.Adm).Include(r => r.Users).ToListAsync();
            if (room.Count == 0)
            {
                return Results.NotFound("Sala não encontrada!");
            }

            var servicesRoom = new ServicesExitRoom();
            foreach (var r in room)
            {
                servicesRoom.DeleteRoom(context, r.Adm, r);
            }

            return Results.Ok("Sala deletada com êxito!!");
        });
    }
}

