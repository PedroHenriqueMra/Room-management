using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using MinimalApi.DbSet.Models;
using Mysqlx;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Endpoints.ConfigureRooms;
using MinimalApi.Endpoints.ConfigureRegisters;
using MinimalApi.Endpoints.ConfigureMessages;

namespace MinimalApi.Endpoints;
public static class ConfigurationRoute
{
    public static void RoutesEndPoints(this WebApplication app)
    {
        // listagens de salas e usuarios
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

        // endpoints register:
        app.EndPointsRegisters();

        // endpoints rooms:
        app.EndpointsRooms();

        // endpoints message:
        app.EndpointsMessages();
    }
}
