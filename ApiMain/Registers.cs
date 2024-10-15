using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Services.ServicesRoom.Delete;
using Services.ServicesUser.Change;

namespace MinimalApi.Endpoints.ConfigureRegisters;
public static class Registers
{
    public static void EndPointsRegisters(this WebApplication app)
    {
        // app.MapPost("/new", async (DbContextModel context, User user) =>
        // {
        //     var existingUser = await context.User
        //     .Where(u => u.Email == user.Email || u.Name == user.Name)
        //     .FirstOrDefaultAsync();
        //     if (existingUser != null)
        //     {
        //         // email ou nome ja existentes!
        //         return Results.BadRequest("Email ou nome já existente!!");
        //     }

        //     var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
        //     var newUser = new User()
        //     {
        //         Name = user.Name,
        //         Email = user.Email,
        //         Password = hashPassword
        //     };
        //     try
        //     {
        //         await context.User.AddAsync(newUser);
        //         await context.SaveChangesAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Exeption: {ex}");
        //         return Results.BadRequest("An exeption occured");
        //     }

        //     return Results.Created($"{newUser.Id}", newUser.Id);
        // });

        // editar usuario pelo id
        // app.MapPut("/edit/{type}/{id:int}", async (ILogger<Program> log, DbContextModel context, string type, int id, UserChangeData updateData) =>
        // {
        //     var user = await context.User.FindAsync(id);
        //     if (user == null)
        //     {
        //         return Results.NotFound("Usuário não encontrado!!");
        //     }

        //     var validationContext = new ValidationContext(updateData);
        //     var validationResults = new List<ValidationResult>();

        //     // Valida o modelo
        //     bool isValid = Validator.TryValidateObject(updateData, validationContext, validationResults, true);
        //     if (!isValid)
        //     {
        //         return Results.BadRequest($"Invalid date to change date");
        //     }

        //     var serviceChange = new ServiceUserChange();
        //     var result = await serviceChange.ChangeWithTypeAsync(log, context, user, updateData, type);
        //     if (result is IStatusCodeHttpResult statusCode && statusCode.StatusCode != 200)
        //     {
        //         log.LogError($"Error ocurred in change {type}. The status code is different of 200");
        //         return Results.BadRequest("An error ocurred in service");
        //     }

        //     return Results.Ok($"Dados editados com êxito!!, {result}");
        // });

        // deletar usuario pelo id
        // app.MapDelete("/delete/{id:int}", [Authorize] async (DbContextModel context, int id) =>
        // {
        //     var user = await context.User.FindAsync(id);
        //     if (user == null)
        //     {
        //         return Results.NotFound("Usuário não encontrado!!");
        //     }

        //     // deletar as rooms
        //     var rooms = await context.Room.Include(r => r.Adm).Where(r => r.UserName.Contains(user.Name)).ToListAsync();
        //     if (rooms.Any())
        //     {
        //         var roomService = new ServicesExitRoom();
        //         foreach (var r in rooms)
        //         {
        //             if (r.Adm.Id == user.Id)
        //             {
        //                 await roomService.DeleteRoom(context, user, r);
        //                 context.Entry(r).State = EntityState.Deleted;
        //             }
        //             else
        //             {
        //                 await roomService.ExitUserRoomAsync(context, user, r);
        //                 context.Entry(r).State = EntityState.Deleted;
        //             }
        //         }
        //     }

        //     context.Remove(user);
        //     context.Entry(user).State = EntityState.Deleted;
        //     await context.SaveChangesAsync();

        //     return Results.Ok("Usuário removido com êxito!!");
        // });

        app.MapDelete("/delete/all", async (DbContextModel context) =>
        {
            foreach (var item in context.User)
            {
                try
                {
                    context.User.Remove(item);
                }
                catch{}
            }
            foreach (var item in context.Room)
            {
                try
                {
                    context.Room.Remove(item);
                }
                catch{}
            }
            foreach (var item in context.Message)
            {
                try
                {
                    context.Message.Remove(item);
                }
                catch{}
            }
            context.SaveChanges();
        });
    }
}
