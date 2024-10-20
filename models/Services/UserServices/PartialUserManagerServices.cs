using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using MinimalApi.Services.Utils.RegularExpression.User;

namespace Services.ServicesUser.Change;

public partial class UserManageServices
{
    private async Task<User> GetUserAsync(int userId)
    {
        var user = await _context.User.FindAsync(userId);
        if (user != null)
        {
            return user;
        }

        return null;
    }

    private string LogicValidationName(User user, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || user.Name == name)
        {
            var message = user.Name == name
                ? $"The name {user.Name} is already yours!!"
                : "The email is null or empty!";
            _logger.LogWarning(message);
            return message;
        }

        var isValidName = UserCheckRegularExpression.IsValidName(name);
        if (!isValidName)
        {
            var message = $"The characters of {name} are invalids!";

            _logger.LogWarning($"The characters regex of {name} are invalids!");
            return message;
        }

        return null;
    }

    private async Task<string> LogicValidationEmail(User user, string email)
    {
        if (string.IsNullOrWhiteSpace(email) || user.Email == email)
        {
            var message = user.Email == email
            ? $"You already is using the email {email}, choose another please"
            : "The email is null or empty!";

            _logger.LogWarning(message);
            return message;
        }

        var emailRegexIsValid = UserCheckRegularExpression.IsValidEmail(email);
        if (!emailRegexIsValid)
        {
            _logger.LogWarning($"The email {email} don't matches with the regex rules of system");
            return $"Invalid characters for {email}!";
        }

        if (await _context.User.AnyAsync(u => u.Email == email))
        {
            _logger.LogWarning($"The email {email} already is in using");
            return $"The email {email} already is in using";
        }

        return null;
    }

    private async Task<bool> ReplaceClaims(string newEmail, int id)
    {
        if (_httpContext.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User isn't authenticated!");
            return false;
        }

        await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var userClaim = _httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (userClaim == null)
        {
            _logger.LogWarning("User claims not found");
            return false;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, newEmail),
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
        };

        var identity = new ClaimsIdentity(claims, "ApplicationCookie");
        var principal = new ClaimsPrincipal(identity);

        try
        {
            await _httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            _logger.LogInformation($"User claims replaced successfully. New email: {newEmail}, UserID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to replace user claims. Error: {ex.Message}");
            return false;
        }
    }
}
