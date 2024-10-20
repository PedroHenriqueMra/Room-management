public interface IMessageServices
{
    Task<IResult> SendMessageAsync(int userId, Guid uuidRoom, string message);
}
