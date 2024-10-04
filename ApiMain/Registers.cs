using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ModelTables;

namespace MinimalApi.Endpoints.ConfigureRegisters;
public static class Registers
{
    public static void EndPointsRegisters(this WebApplication app)
    {
        app.MapPost("/new", async (DbContextModel context, User user) =>
        {
            var existingUser = await context.User
            .Where(u => u.Email == user.Email || u.Name == user.Name)
            .FirstOrDefaultAsync();
            if (existingUser != null)
            {
                // email ou nome ja existentes!
                return Results.BadRequest("Email ou nome já existente!!");
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var newUser = new User()
            {
                Name = user.Name,
                Email = user.Email,
                Password = hashPassword
            };
            try
            {
                await context.User.AddAsync(newUser);
                await context.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exeption: {ex}");
                return Results.BadRequest("An exeption occured");
            }

            return Results.Created($"{newUser.Id}", newUser.Id);
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
            for (var l = 0; l < rooms.Count; l++)
            {
                var index = rooms[l].UsersNames.IndexOf(user.Name);
                if (index != -1)
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

            return Results.Ok("Dados editados com êxito!!");
        });

        // deletar usuario pelo id
        app.MapDelete("/delete/{id:int}", [Authorize] async (DbContextModel context, int id) =>
        {
            var user = await context.User.FindAsync(id);
            if (user == null)
            {
                return Results.NotFound("Usuário não encontrado!!");
            }

            // deletar as rooms
            var rooms = await context.Room.Include(r => r.Adm).Where(r => r.UsersNames.Contains(user.Name)).ToListAsync();
            if (rooms.Any())
            {
                var roomService = new ServicesRoomDelete();
                foreach (var r in rooms)
                {
                    if (r.Adm.Id == user.Id)
                    {
                        await roomService.DeleteRoom(context, user, r);
                        context.Entry(r).State = EntityState.Deleted;
                    }
                    else
                    {
                        await roomService.ExitUserRoomAsync(context, user, r);
                        context.Entry(r).State = EntityState.Deleted;
                    }
                }
            }

            context.Remove(user);
            context.Entry(user).State = EntityState.Deleted;
            await context.SaveChangesAsync();

            return Results.Ok("Usuário removido com êxito!!");
        });
    }
}
