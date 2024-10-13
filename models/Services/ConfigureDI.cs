using Microsoft.CodeAnalysis.CSharp.Syntax;
using Services.ServicesRoom.Delete;
using Services.ServicesRoom.Enter;
using Services.ServicesUser.Change;

namespace Services.Configuration.DI;

public static class ConfigureDI
{
    public static void AddAllDependencies(this IServiceCollection service)
    {
        service.AddHttpClient();
        service.AddHttpContextAccessor();

        // Rooms services:
        // Entrance and exit of rooms
        service.AddSingleton<IServicesEnterRoom, ServicesEnterRoom>();
        service.AddSingleton<IServicesExitRoom, ServicesExitRoom>();

        // User services:
        // Change user data
        service.AddSingleton<IServiceUserChange, ServiceUserChange>();
    }
}
