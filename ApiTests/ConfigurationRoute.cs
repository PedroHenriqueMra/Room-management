using MinimalApi.DbSet.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiTests.Endpoints
{
    internal static class ConfigurationRoute
    {
        internal static void RoutesEndPoints(this WebApplication app)
        {
            // users, rooms list
            app.MapGet("/list", async (DbContextModel context) =>
            {
                var users = await context.User.ToListAsync();
                var rooms = await context.Room.ToListAsync();

                var display = new Dictionary<string, object>
                {
                    { "Users", users },
                    { "Rooms", rooms }
                };

                return Results.Ok(display);
            });

            // list chat connections
            app.MapGet("/list/chat", async (DbContextModel context) =>
            {
                var chatGroups = await context.ChatGroup.ToListAsync();
                var display = new Dictionary<string, object>
                {
                    { "Connections", chatGroups }
                };

                return Results.Ok(display);
            });

            // delete all data
            app.MapDelete("/delete/database", async (DbContextModel context) =>
            {
                foreach (var item in context.User)
                {
                    try
                    {
                        context.User.Remove(item);
                    }
                    catch { }
                }
                foreach (var item in context.Room)
                {
                    try
                    {
                        context.Room.Remove(item);
                    }
                    catch { }
                }
                foreach (var item in context.Message)
                {
                    try
                    {
                        context.Message.Remove(item);
                    }
                    catch {}
                }
                foreach (var item in context.ChatGroup)
                {
                    try
                    {
                        context.ChatGroup.Remove(item);
                    }
                    catch {}
                }
                context.SaveChanges();
            });

            // generate a test account (for tests)
            // preferably generate the test after clean database
            app.MapGet("/gen/test", async (DbContextModel context) =>
            {
                try
                {
                    User testAccount = new User 
                    {
                        Name = "test",
                        Email = "systemtest@gmail.com",
                        // choose your own password
                        Password = "Phh12345",
                    };

                    testAccount.Password = BCrypt.Net.BCrypt.HashPassword(testAccount.Password);

                    await context.User.AddAsync(testAccount);
                    await context.SaveChangesAsync();
                    
                    return Task.FromResult(testAccount);
                }
                catch {}
                
                return Task.FromException(new ArgumentException("A possible data confict ocurred while testAccount creation"));
            }); 
        }
    }
}

