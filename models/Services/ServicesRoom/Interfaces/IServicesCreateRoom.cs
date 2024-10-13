using MinimalApi.DbSet.Models;

public interface IServicesCreateRoom
{
    Task<IResult> CreateRoomAsync(CreateRoomRequest data);
}
