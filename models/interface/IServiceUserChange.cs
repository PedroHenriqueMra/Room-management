using MinimalApi.DbSet.Models;

public interface IServiceUserChange
{
    Task<IResult> NameChange(DbContextModel context, User userChange, string newName);

    Task<IResult> EmailChange(DbContextModel context, User userChange, string newEmail);

    Task<IResult> PasswordChange(DbContextModel context, User userChange, string newPassword);
}
