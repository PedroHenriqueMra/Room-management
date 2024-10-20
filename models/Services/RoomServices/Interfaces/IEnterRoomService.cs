using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

public interface IEnterRoomService
{
    Task<IResult> IncludeUserAsync(ILogger log, DbContextModel context, ParticipeRoomData data);
}
