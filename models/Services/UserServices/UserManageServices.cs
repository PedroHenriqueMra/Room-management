using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MinimalApi.DbSet.Models;
using MySqlX.XDevAPI.Common;
using ZstdSharp.Unsafe;
using MinimalApi.Services.Utils.RegularExpression.User;

namespace Services.ServicesUser.Change;

public partial class UserManageServices : IUserManageServices
{
    private static readonly List<string> AllowedFields = new List<string>
    {
        "name", "email", "password"
    };

    private readonly HttpContext _httpContext;
    private readonly ILogger<UserManageServices> _logger;
    private readonly DbContextModel _context;
    public UserManageServices(IHttpContextAccessor httpContext, ILogger<UserManageServices> logger, DbContextModel context)
    {
        var http = httpContext.HttpContext;
        if (http != null)
        {
            _httpContext = http;
        }

        _logger = logger;
        _context = context;
    }

    public async Task<IResult> NameChangeAsync(int id, string name)
    {
        var user = await GetUserAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"The user whose id is {id} not found");
            return Results.NotFound($"The user which id is {id} not found!");
        }

        var logicValidationName = LogicValidationName(user, name);
        if (!String.IsNullOrEmpty(logicValidationName))
        {
            _logger.LogWarning($"Some error ocurred in logic validation name. Message: {logicValidationName}");
            return Results.BadRequest(logicValidationName);
        }

        try
        {
            var rooms = await _context.Room.Where(r => r.Users.Any(u => u.Id == id)).ToListAsync();
            foreach (var r in rooms)
            {
                int index = r.UserName.IndexOf(user.Name);
                if (index != -1)
                {
                    r.UserName[index] = name;
                    _logger.LogInformation($"Updated user name from {user.Name} to {name} in room {r.Name}");
                }
                else
                {
                    _logger.LogWarning($"User {user.Name} not found in room {r.Name}");
                }
                _context.Room.Update(r);
            }

            user.Name = name;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Error updating name for UserId: {id}. Exception: {ex}");
            return Results.BadRequest("An error ocurred");
        }

        _logger.LogInformation("User was edited successfully!!");
        return Results.Ok(new { message = "User edited", newData = name });
    }

    public async Task<IResult> EmailChangeAsync(int id, string email)
    {
        var user = await GetUserAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"The user whose id is {id} not found");
            return Results.NotFound($"The user which id is {id} not found!");
        }

        var logicValidationEmail = await LogicValidationEmail(user, email);
        if (!String.IsNullOrEmpty(logicValidationEmail))
        {
            _logger.LogWarning($"Some error ocurred in logic validation name. Message: {logicValidationEmail}");
            return Results.BadRequest(logicValidationEmail);
        }

        using var transaction = await _context.Database.BeginTransactionAsync(); // rollback
        try
        {
            var originalEmail = user.Email;
            user.Email = email;
            await _context.SaveChangesAsync();

            bool claimsUpdate = await ReplaceClaims(email, user.Id);
            if (!claimsUpdate)
            {
                throw new Exception("Failed to update claims");
            }

            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogCritical("Transaction failed: {Error}", ex.Message);
            return Results.BadRequest(new { error = "An error occurred during email update" });
        }

        _logger.LogInformation($"Email update complete. New email: {email}");
        return Results.Ok(new { message = "User edited", newData = email });
    }

    public async Task<IResult> PasswordChangeAsync(int id, string password)
    {
        return Results.Ok();
    }
}
