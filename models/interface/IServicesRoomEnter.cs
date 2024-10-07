using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

public interface IServicesRoomEnter
{
    Task<IResult> IncludeUserAsync(DbContextModel context, User user, Room room);
    Task<IResult> CheckDataAsync(DbContextModel context, User user, Room room);
}
