using MinimalApi.DbSet.Models;

public interface IRoomCreateService
{
    Task<IResult> CreateRoomAsync(CreateRoomRequest data);
}
