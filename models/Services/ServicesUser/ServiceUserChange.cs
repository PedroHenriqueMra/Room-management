using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MinimalApi.DbSet.Models;
using MySqlX.XDevAPI.Common;
using ZstdSharp.Unsafe;

namespace Services.ServicesUser.Change;

public class ServiceUserChange : IServiceUserChange
{
    private static readonly List<string> AllowedFields = new List<string>
    {
        "name", "email", "password"
    };

    private readonly IHttpContextAccessor _httpContext;
    public ServiceUserChange(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public async Task<IResult> ChangeWithTypeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData value, string type)
    {
        if (AllowedFields.Contains(type))
        {
            if (type == "name")
            {
                return await NameChangeAsync(log, context, userChange, value);
            }
            else if (type == "email")
            {
                return await EmailChangeAsync(log, context, userChange, value);
            }
            else
            {
                return await PasswordChangeAsync(log, context, userChange, value);
            }
        }

        log.LogError($"The type {type} don't existe!!");
        return Results.BadRequest($"The request type '{type}' isn't valid!");
    }

    public async Task<IResult> NameChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newData)
    {
        if (string.IsNullOrWhiteSpace(newData.Name))
        {
            log.LogWarning("The newName is null or empty");
            return Results.BadRequest("Name value is null or empty");
        }

        if (userChange.Name == newData.Name)
        {
            log.LogInformation($"newName = {newData.Name} is the same userName = {userChange.Name}");
            return Results.Conflict($"The name {newData.Name} is already yours!!");
        }

        try
        {
            var rooms = await context.Room.Where(r => r.Users.Any(u => u.Id == userChange.Id))
            // .Select(r => new { r.Name, r.UserName })
            .ToListAsync();
            foreach (var r in rooms)
            {
                int index = r.UserName.IndexOf(userChange.Name);
                if (index != -1)
                {
                    r.UserName[index] = newData.Name;
                    log.LogInformation($"Updated user name from {userChange.Name} to {newData.Name} in room {r.Name}");
                }
                else
                {
                    log.LogWarning($"User {userChange.Name} not found in room {r.Name}");
                }
                context.Room.Update(r);
            }

            userChange.Name = newData.Name;
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            log.LogError($"Error updating name for UserId: {userChange.Id}. Exception: {ex}");
            return Results.BadRequest("An error ocurred");
        }

        log.LogInformation("User was edited successfully!!");
        return Results.Ok(new { message = "User edited", newData = userChange.Name });
    }







    public async Task<IResult> EmailChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newData)
    {
        if (string.IsNullOrWhiteSpace(newData.Email))
        {
            log.LogWarning("The newEmail is null or empty");
            return Results.BadRequest("Email value is null or empty");
        }
        if (userChange.Email == newData.Email)
        {
            log.LogInformation($"newName = {newData.Email} is the same userName = {userChange.Email}");
            return Results.Conflict($"The name {newData.Email} is already yours!!");
        }
        if (await context.User.AnyAsync(u => u.Email == newData.Email))
        {
            log.LogWarning($"The email {newData.Email} is already in use by another user.");
            return Results.Conflict($"The email {newData.Email} is already in use by another user.");
        }

        try
        {
            userChange.Email = newData.Email;
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            log.LogError($"An error ocurred in database. Message {ex}");
            return Results.BadRequest(new { error = "An error occurred while updating the email. Please try again later." });

        }

        log.LogInformation($"Email update complete. New email: {userChange.Email}");
        return Results.Ok(new { message = "User edited", newData = userChange.Email });
    }

    private bool AtualizeClaims(string oldEmail, string newEmail)
    {
        var userClaim = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (userClaim != null)
        {
            return true;
        }

        return false;
    }






    public async Task<IResult> PasswordChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newData)
    {
        return Results.Ok();
    }
}
