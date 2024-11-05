using Microsoft.EntityFrameworkCore;
using ApiTests.Endpoints;
using MinimalApi.Services.Configuration.DI;
using MinimalApi.Services.Authentication;
using MinimalApi.Chat;
// using MinimalApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure DIs
builder.Services.AddAllDependencies();

builder.Services.AddDbContext<DbContextModel>(
    options => options.UseSqlite("Data Source=adjisajd.db"));

// Configure authentication
builder.Services.ConfigureIdentity();
builder.Services.ConfigureCookie();

builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("https://http://localhost:5229")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// api service
app.RoutesEndPoints();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// app.UseMiddleware<MyMiddleware>();

app.MapRazorPages();
// map SignalR
app.MapHub<ChatSignalR>("/chat");

app.UseEndpoints(endpoint => {
    endpoint.MapRazorPages();
});

app.Run();
