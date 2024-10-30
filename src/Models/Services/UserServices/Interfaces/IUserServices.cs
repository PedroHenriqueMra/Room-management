using MinimalApi.DbSet.Models;

public  interface IUserServices
{
    Task<IResult> UserCreateAsync(User user);
    Task<IResult> UserLoginAsync(string password, string email);
}
