using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Tls;

namespace Services.ServicesRoom.Enter;

public class ServicesEnterRoom : IServicesEnterRoom
{
    public async Task<IResult> IncludeUserAsync(ILogger log, DbContextModel context, User user, Room room)
    {
        var checkData = await CheckDataAsync(log, context, user, room);
        if (checkData is IStatusCodeHttpResult statusCode && statusCode.StatusCode != 200)
        {
            log.LogWarning("Check datas error");
            return checkData;
        }

        try
        {
            room.UserName.Add(user.Name);
            room.Users.Add(user);
            user.RoomsNames.Add(room.Name);
            user.Rooms.Add(room);

            await context.SaveChangesAsync();
        }
        catch (Exception Ex)
        {
            log.LogWarning($"Message error: {Ex.Message}");
            return Results.StatusCode(500);
        }

        log.LogWarning("User entered successfully");
        return Results.Ok(new { Message = "User entered successfully", Room = room, User = user });
    }

    private async Task<IResult> CheckDataAsync(ILogger log, DbContextModel context, User user, Room room)
    {
        var existeRoom = await context.Room.FirstOrDefaultAsync(r => r.Id == room.Id);
        if (existeRoom == null)
        {
            log.LogWarning("Room not existes");
            return Results.NotFound("Room doesn't existes");
        }

        if (room.IsPrivate)
        {
            log.LogWarning("The room is private");
            return Results.Forbid();
        }

        if (room.Users.Any(u => u.Id == user.Id))
        {
            log.LogInformation($"User {user.Name} is already in the room");
            return Results.Conflict("User already existes");
        }

        log.LogInformation("Data checks passed");
        return Results.Ok(new { message = "User entered", UserName = user.Name });
    }
}
