using MinimalApi.DbSet.Models;

public interface IUserManageServices
{
    Task<IResult> ChangeWithTypeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newData, string type);

    Task<IResult> NameChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newName);

    Task<IResult> EmailChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newEmail);

    Task<IResult> PasswordChangeAsync(ILogger log, DbContextModel context, User userChange, UserChangeData newPassword);
}
