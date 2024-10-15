using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

public interface IServicesEnterRoom
{
    Task<IResult> IncludeUserAsync(ILogger log, DbContextModel context, ParticipeRoomData data);
}
