public interface IMessageServices
{
    Task<IResult> CreateMessageAsync(int? userId, Guid? uuidRoom, string? message);
}
