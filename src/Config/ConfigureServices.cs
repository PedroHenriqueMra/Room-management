using Microsoft.CodeAnalysis.CSharp.Syntax;
using Services.ServicesRoom.Delete;
using Services.ServicesRoom.Enter;
using Services.ServicesUser.Change;
using Services.RoomServices.Create;
using MinimalApi.Chat.Services;

namespace MinimalApi.Services.Configuration.DI;

public static class ConfigureDI
{
    public static void AddAllDependencies(this IServiceCollection service)
    {
        service.AddHttpClient();
        service.AddHttpContextAccessor();

        // Rooms services:
        // Entrance and exit of rooms
        service.AddSingleton<IEnterRoomService, EnterRoomService>();
        service.AddScoped<IExitRoomService, ExitRoomService>();
        // Create room
        service.AddScoped<IRoomCreateService, RoomCreateService>();

        // User services:
        // User create, Login
        service.AddScoped<IUserServices, UserServices>();
        // Manager user
        service.AddScoped<IUserManageServices, UserManageServices>();

        // Message services:
        service.AddScoped<IMessageServices, MessageServices>();

        // Service get message error:
        service.AddSingleton<IGetPropertyAnonymous, GetPropertyAnonymous>();

        // Service hub
        service.AddScoped<IChatService, ChatHubService>();
    }
}
