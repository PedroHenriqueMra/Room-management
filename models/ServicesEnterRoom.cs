using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Tls;

public class ServicesEnterRoom : IServicesRoomEnter
{
    public async Task<IResult> IncludeUserAsync(DbContextModel context, User user, Room room)
    {
        var checkData = await CheckDataAsync(context, user, room);
        if (checkData is IStatusCodeHttpResult statusCode && statusCode.StatusCode != 200)
        {
            Console.WriteLine("Check datas error");
            return checkData;
        }

        try
        {
            room.UsersNames.Add(user.Name);
            room.Users.Add(user);
            user.RoomsNames.Add(room.Name);
            user.Rooms.Add(room);

            await context.SaveChangesAsync();
        }
        catch (Exception Ex)
        {
            Console.WriteLine($"Message error: {Ex.Message}");
            return Results.StatusCode(500);
        }

        Console.WriteLine("User entered successfully");
        return Results.Ok(new { Message = "User entered successfully", Room = room, User = user });
    }

    public async Task<IResult> CheckDataAsync(DbContextModel context, User user, Room room)
    {
        var existeRoom = await context.Room.FirstOrDefaultAsync(r => r.Id == room.Id);
        if (existeRoom == null)
        {
            Console.WriteLine("Room not existes");
            return Results.NotFound("Room doesn't existes");
        }

        if (room.IsPrivate)
        {
            Console.WriteLine("The room is private");
            return Results.Forbid();
        }

        if (room.UsersNames.Contains(user.Name))
        {
            Console.WriteLine($"User {user.Name} is already in the room");
            return Results.Conflict("User already existes");
        }

        Console.WriteLine("Data checks passed");
        return Results.Ok();
    }
}
