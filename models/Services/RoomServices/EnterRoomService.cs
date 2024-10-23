using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Tls;
using MinimalApi.Services.Utils.RegularExpression.Room;

namespace Services.ServicesRoom.Enter;

public class EnterRoomService : IEnterRoomService
{
    public async Task<IResult> IncludeUserAsync(ILogger log, DbContextModel context, ParticipeRoomData participeData)
    {
        var checkData = await CheckDataAsync(log, context, participeData.User, participeData.Room, participeData.Password);
        if (checkData is IStatusCodeHttpResult statusCode && statusCode.StatusCode > 299)
        {
            log.LogWarning($"Check datas error, StatusCode: {statusCode}.");
            return checkData;
        }

        if (participeData.Room.Users.Any(u => u.Id == participeData.User.Id))
        {
            log.LogWarning($"The user {participeData.User.Name} already existe in room {participeData.Room.Name}!");
            return Results.Conflict("User  already existe in this room!");
        }

        try
        {
            if (!participeData.Room.Users.Contains(participeData.User))
            {
                participeData.Room.Users.Add(participeData.User);
                participeData.Room.UserName.Add(participeData.User.Name);
            
                participeData.User.Rooms.Add(participeData.Room);
                participeData.User.RoomsNames.Add(participeData.Room.Name);

                await context.SaveChangesAsync();
            }
        }
        catch (Exception Ex)
        {
            log.LogWarning($"Message error: {Ex.Message}");
            return Results.StatusCode(500);
        }

        log.LogWarning("User entered successfully");
        return Results.Ok(new { Message = "User entered successfully", Room = participeData.Room, User = participeData.User });
    }

    private async Task<IResult> CheckDataAsync(ILogger log, DbContextModel context, User user, Room room, string? password)
    {
        var existeRoom = await context.Room.FirstOrDefaultAsync(r => r.Id == room.Id);
        if (!await context.Room.AnyAsync(r => r.Id == room.Id))
        {
            log.LogWarning("Room not existes");
            return Results.NotFound("Room doesn't existes");
        }

        if (room.Users.Any(u => u.Id == user.Id))
        {
            log.LogInformation($"User {user.Name} is already in the room");
            return Results.Conflict("User already is into the room");
        }

        if (room.IsPrivate)
        {
            if (password == null)
            {
                log.LogInformation($"This room is private and not contains a password");
                return Results.BadRequest("A password is required for this room!");
            }

            bool regexPassword = RoomCheckRegularExpression.RoomPasswordIsValid(password);
            if (!regexPassword)
            {
                log.LogInformation($"Invalid character for {password}");
                return Results.BadRequest($"Error of character for {password}");
            }

            if (room.Password != password)
            {
                log.LogInformation($"Incorrect password for room {room.Name}");
                return Results.BadRequest($"Incorrect Password, try another");
            }
        }

        log.LogInformation("Data checks passed");
        return Results.Ok(new { message = "User entered", UserName = user.Name });
    }
}
