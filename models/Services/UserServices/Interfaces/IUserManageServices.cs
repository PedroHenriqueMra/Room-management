using MinimalApi.DbSet.Models;

public interface IUserManageServices
{
    Task<IResult> NameChangeAsync(int id, string name);
    Task<IResult> EmailChangeAsync(int id, string email);
    Task<IResult> PasswordChangeAsync(int id, string password);
}
