using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using ModelTables;
using Mysqlx;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Generators;

namespace ServicesApp;
public static class ConfigurationRoute
{
    public static void RoutesEndPoints(this WebApplication app)
    {
        // listagens de salas
        app.MapGet("/tolist", async (DbContextModel context) =>
        {
            var users = await context.User.ToListAsync();
            var rooms = await context.Room.ToListAsync();

            var display = new Dictionary<string, object>{
                { "Users", users },
                { "Rooms", rooms }
            };

            return Results.Ok(display);
        });

        // inserir dados do novo registro (usuario)
        app.MapPost("/new", async (DbContextModel context, User user) =>
        {
            var queryEmail = await context.User.FirstOrDefaultAsync(q => q.Email == user.Email);
            var queryName = await context.User.FirstOrDefaultAsync(q => q.Name == user.Name);
            if (queryEmail != null || queryName != null)
            {
                // email ou nome ja existentes!
                return false;
                // return Results.BadRequest("Email ou nome já existente!!");
            }
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var newUser = new User()
            {
                Name = user.Name,
                Email = user.Email,
                Password = hashPassword
            };
            await context.AddAsync(newUser);
            await context.SaveChangesAsync();
            return true;
            // return Results.Ok("Usuário registrado com êxito!!");
        });

        // editar usuario pelo id
        app.MapPut("/edit/{id:int}", [Authorize] async (DbContextModel context, int id, User updateData) =>
        {
            var user = await context.User.FindAsync(id);

            if (user == null)
            {
                return Results.NotFound("Usuário não encontrado!!");
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(updateData.Password);

            // mudar o nome do usuario em todas as salas:
            var rooms = await context.Room.Where(r => r.UsersNames.Contains(user.Name)).ToListAsync();
            for (var l = 0;l < rooms.Count; l++)
            {
                var index = rooms[l].UsersNames.IndexOf(user.Name);
                if(index != -1)
                {
                    rooms[l].UsersNames[index] = updateData.Name;
                    context.Entry(rooms[l]).State = EntityState.Modified;
                }
            }
            
            user.Name = updateData.Name;
            user.Email = updateData.Email;
            user.Password = hashPassword;
            context.Entry(user).State = EntityState.Modified;

            await context.SaveChangesAsync();

            // return true;
            return Results.Ok("Dados editados com êxito!!");
        });

        // deletar usuario pelo id
        app.MapDelete("/delete/{id:int}", [Authorize] async (DbContextModel context, int id) =>
        {
            var user = await context.User.FindAsync(id);
            if (user == null)
            {
                // return false;
                return Results.NotFound("Usuário não encontrado!!");
            }

            context.Remove(user);
            await context.SaveChangesAsync();

            // return true;
            return Results.Ok("Usuário removido com êxito!!");
        });

        // criar uma nova sala
        app.MapPost("/new/room", async (DbContextModel context, RoomRequest roomData) =>
        {
            var adm = await context.User.FindAsync(roomData.IdUser);
            if (adm == null)
            {
                // return false;
                return Results.NotFound("Usuário não encontrado!");
            }

            var existingRoom = await context.Room.FirstOrDefaultAsync(r => r.Name == roomData.Name);
            if (existingRoom != null)
            {

                // return false;
                return Results.BadRequest("Já existe uma sala com este nome!!");
            }

            var newRoom = new Room()
            {
                Name = roomData.Name,
                Adm = adm,
                Description = roomData.Description,
                AdmId = adm.Id
            };
            newRoom.Users.Add(adm);
            newRoom.UsersNames.Add(adm.Name);
            adm.Rooms.Add(newRoom);
            adm.RoomsNames.Add(newRoom.Name);

            await context.AddAsync(newRoom);
            await context.SaveChangesAsync();

            // return true;

            return Results.Ok("Sala criada com êxito!!");
        });

        // exibir conteudo da sala selecionada
        app.MapGet("/room/{uuid:guid}", [Authorize] async (DbContextModel context, Guid uuid) =>
        {
            var room = await context.Room.FindAsync(uuid);
            if (room == null)
            {
                return Results.NotFound("Sala não encontrada!");
            }

            return Results.Ok(room.Messages);
        });

        // deletar uma sala (apenas adm)
        app.MapDelete("/room/delete/{uuid:guid}/{id:int}", [Authorize] async (DbContextModel context, Guid uuid, int id) =>
        {
            var room = await context.Room.FindAsync(uuid);
            if (room == null)
            {
                // return false;
                return Results.NotFound("Sala não encontrada!");
            }

            var adm = await context.User.FindAsync(id);
            if (adm == null)
            {
                // return false;
                return Results.NotFound("Usuário não encontardo!!");
            }

            if (room.AdmId != adm.Id)
            {
                // return false;
                return Results.BadRequest("O usuário não contem as permissões necessárias para esta ação!!");
            }

            var servicesRoom = new ServicesRoomDeleteRoom();
            servicesRoom.DeleteRoom(context, adm, room);

            // return true;
            return Results.Ok("Sala deletada com êxito!!");
        });

        // participar de uma sala
        app.MapPost("/room/participate/{uuid:guid}", [Authorize] async (DbContextModel context, Guid uuid) =>
        {
            // com autenticação
        });

        // sair de uma sala
        app.MapPut("/room/exit/", async (DbContextModel context) =>
        {
            // com autenticação
        });
    }
}
