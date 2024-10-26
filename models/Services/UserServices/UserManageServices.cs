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
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Cms;
using Azure;

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

        if (user.Name == name || String.IsNullOrEmpty(name))
        {
            _logger.LogInformation("The user didn't change your name!");
            return Results.NoContent();
        }

        var logicValidationName = await LogicValidationNameAsync(name);
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
        if (user.Email == email || String.IsNullOrEmpty(email))
        {
            _logger.LogWarning($"The user didn't change your email!");
            return Results.NoContent();
        }

        var logicValidationEmail = await LogicValidationEmailAsync(email);
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
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning($"The user which id {id} not found!");
            return Results.BadRequest(new { message = $"User with ID {id} not found." });
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            _logger.LogInformation("The password wasan't chanded!");
            return Results.BadRequest(new { message = "The user password wasan't changed!" });
        }

        if (!UserCheckRegularExpression.IsValidPassword(password))
        {
            _logger.LogWarning($"Password regex error for {password}");
            return Results.Conflict(new { message = $"Some character is wrong in password!" });
        }

        try
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning($"An error ocurred while the data was changed on database!. Message: {ex}");
            return Results.BadRequest(new { message = "An internal server error with database ocurred!" });
        }

        _logger.LogInformation("The user password was changed!");
        return Results.Ok(new { message = "The user password was changed successfully" });
    }
}
