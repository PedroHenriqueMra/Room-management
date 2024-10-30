using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using MinimalApi.Services.Utils.RegularExpression.User;
using SQLitePCL;
using MinimalApi.Services.Utils.RegularExpression.User;

public class UserServices : IUserServices
{
    protected readonly DbContextModel _context;
    public UserServices(DbContextModel context)
    {
        _context = context;
    }

    public async Task<IResult> UserCreateAsync(User user)
    {
        bool isValidData = UserCheckRegularExpression.CheckAll(user.Password, user.Email, user.Name);
        if (!isValidData)
        {
            return Results.BadRequest($"One or more fields are invalid");
        }

        var existingUser = await _context.User
            .Where(u => u.Email == user.Email || u.Name == user.Name)
            .FirstOrDefaultAsync();
        if (existingUser != null)
        {
            // email ou nome ja existentes!
            return Results.BadRequest("Email ou nome já existente!!");
        }

        var hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
        var newUser = new User()
        {
            Name = user.Name,
            Email = user.Email,
            Password = hashPassword
        };
        try
        {
            await _context.User.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exeption: {ex}");
            return Results.BadRequest("An exeption occured");
        }

        return Results.Created($"{newUser.Id}", newUser.Id);
    }

    public async Task<IResult> UserLoginAsync(string password, string email)
    {
        bool checkPassword = UserCheckRegularExpression.IsValidPassword(password);
        bool checkEmail = UserCheckRegularExpression.IsValidEmail(email);
        if (!checkEmail || !checkPassword)
        {
            return Results.BadRequest($"Invalid charactere in password {password} or email {email}");
        }

        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Results.BadRequest($"This email not existe");
        }
        
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return Results.BadRequest($"Incorrect password!");
        }
        
        return Results.Ok("Email and password are correct!!");
    }
}
