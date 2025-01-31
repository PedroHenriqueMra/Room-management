using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;
using NuGet.Protocol;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ApiTests.Endpoints.ConfigureMessages;
internal static class Messages
{
    internal static void EndpointsMessages(this WebApplication app)
    {
        // app.MapPost("/new/message", async (DbContextModel context, MessageRequest dataMessage) =>
        // {
        //     if (dataMessage.IdRoom == Guid.Empty || dataMessage.IdUser == null)
        //     {
        //         return Results.BadRequest("Empty data");
        //     }

        //     var user = await context.User.FindAsync(dataMessage.IdUser);
        //     if (user == null)
        //     {
        //         return Results.BadRequest("User not found");
        //     }

        //     var room = await context.Room.FindAsync(dataMessage.IdRoom);
        //     if (room == null)
        //     {
        //         return Results.BadRequest("Room not found");
        //     }

        //     if (dataMessage.Content.Length == 0 || dataMessage.Content.Length > 200)
        //     {
        //         return Results.BadRequest($"Message length must be between 1 and 200 characters. Current length: {dataMessage.Content.Length}");
        //     }

        //     var message = new Message
        //     {
        //         Content = dataMessage.Content,
        //         User = user,
        //         UserId = user.Id,
        //         Room = room,
        //         RoomId = room.Id
        //     };
        //     try
        //     {
        //         await context.Message.AddAsync(message);
        //         user.Messages.Add(message);
        //         room.Messages.Add(message);


        //         await context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         return Results.BadRequest("Database update error occurred while sending the message.");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"An error occurred while sending the message!!: {ex}");
        //         return Results.BadRequest($"An unexpected error ocurred while sending the message.");
        //     }

        //     return Results.StatusCode(201);
        // });
    }
}
