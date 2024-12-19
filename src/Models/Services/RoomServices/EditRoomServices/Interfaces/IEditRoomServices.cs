using CSharpFunctionalExtensions;

public interface IEditRoomNameServices
{
    Task<Result<string>> ChangeName(Guid roomId, string name);
}
