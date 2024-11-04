using Microsoft.AspNetCore.SignalR;

namespace MinimalApi.Chat;
public class ChatSignalR : Hub
{
    public async Task SendMessage(int id, string message, Guid uuid)
    {
        
    }
}
