using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Crypto.Operators;
using MinimalApi.Services.Utils.RegularExpression.Message;
using Newtonsoft.Json;

public class MessageServices : IMessageServices
{
    private readonly DbContextModel _context;
    private readonly ILogger<MessageServices> _logger;

    public MessageServices(DbContextModel context, ILogger<MessageServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IResult> CreateMessageAsync(int? userId, Guid? uuidRoom, string? message)
    {
        if (userId == null || uuidRoom == null || message == null)
        {
            _logger.LogWarning($"Some data is null");
            return Results.BadRequest($"the data is null");
        }
        var user = await _context.User.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User not found with id: {userId}");
            return Results.BadRequest($"User not found by id {userId}");
        }

        var room = await _context.Room.FindAsync(uuidRoom);
        if (room == null)
        {
            _logger.LogWarning($"Room not found with id: {uuidRoom}");
            return Results.BadRequest($"Room not found by id {uuidRoom}");
        }

        var messageIsValid = MessageCheckRegularExpression.MessageIsValid(message);
        if (!messageIsValid)
        {
            _logger.LogWarning($"Invalid message character for {message}");
            return Results.BadRequest("Invalid message character");
        }

        var newMessage = new Message()
        {
            Content = message,
            User = user,
            UserId = user.Id,
            Room = room,
            RoomId = room.Id
        };

        try
        {
            await _context.Message.AddAsync(newMessage);

            user.Messages.Add(newMessage);
            room.Messages.Add(newMessage);

            await _context.SaveChangesAsync();

            _logger.LogInformation("The save completed");

            // send just user email, user name and  mesage content.
            var ContentMessage = new {
                UserEmail = newMessage.User.Email,
                UserName = newMessage.User.Name,
                Content = newMessage.Content
            };

            return Results.Content(JsonConvert.SerializeObject(ContentMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error ocurred at the save data base for Message data: {newMessage}. Message error: {ex}");
            return Results.BadRequest("An error ocurred while saving data in the data base");
        }
    }
}
