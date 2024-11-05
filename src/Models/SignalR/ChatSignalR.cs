using Microsoft.AspNetCore.SignalR;

namespace MinimalApi.Chat;
public class ChatSignalR : Hub
{
    public async Task SendMessage(string message, string user)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
