using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelTables;
using Newtonsoft.Json;
using NuGet.Protocol;
using Org.BouncyCastle.Asn1.Ocsp;

namespace MinimalApi.Endpoints.ConfigureMessages;
public static class Messages
{
    public static void EndpointsMessages(this WebApplication app)
    {
        app.MapPost("/new/message", async (HttpContext http, DbContextModel context, MessageRequest dataMessage) =>
        {
            if (dataMessage.IdRoom == Guid.Empty || dataMessage.IdUser == null)
            {
                return Results.StatusCode(400);
            }
            
            if(http.User.Identity?.IsAuthenticated ?? false)
            {
                return Results.BadRequest("unauthenticated user!!");
            }

            var user = await context.User.FindAsync(dataMessage.IdUser);
            if (user == null)
            {
                return Results.BadRequest("User not found");
            }

            var room = await context.Room.FindAsync(dataMessage.IdRoom);
            if (room == null)
            {
                return Results.BadRequest("Room not found");
            }

            if (dataMessage.Content.Length == 0 || dataMessage.Content.Length > 200)
            {
                Console.WriteLine("tamanho do conteudo");
                return Results.BadRequest($"Error in message length!!. Your message has length of {dataMessage.Content.Length} and is supported with length less than 200 and greater than 0");
            }

            var message = new Message
            {
                Content = dataMessage.Content,
                User = user,
                UserId = user.Id,
                Room = room,
                RoomId = room.Id
            };
            try
            {
                await context.Message.AddAsync(message);
                user.Messages.Add(message);
                room.Messages.Add(message);

                // await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Results.BadRequest("Database update error occurred while sending the message.");
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"An error occurred while sending the message!!: {ex}");
            }

            return Results.StatusCode(201);
        });
    }
}
