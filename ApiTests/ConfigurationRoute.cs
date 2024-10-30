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
using ApiTests.Endpoints.ConfigureMessages;
using ApiTests.Endpoints.ConfigureRegisters;
using ApiTests.Endpoints.ConfigureRooms;

namespace ApiTests.Endpoints
{
    internal static class ConfigurationRoute
    {
        internal static void RoutesEndPoints(this WebApplication app)
        {
            // listagens de salas e usuarios
            app.MapGet("/list", async (DbContextModel context) =>
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
}

