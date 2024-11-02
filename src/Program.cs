using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ApiTests.Endpoints;
using MinimalApi.Services.Configuration.DI;
using MinimalApi.Services.Authentication;
// using MinimalApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure DIs
builder.Services.AddAllDependencies();

builder.Services.AddDbContext<DbContextModel>(
    options => options.UseSqlite("Data Source=adjisajd.db"));

// Configure authentication
builder.Services.ConfigureIdentity();
builder.Services.ConfigureCookie();

// builder.Services.AddRazorPages(opt => {
    // desabilita o token de formulario obrigatorio
    // opt.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
// });

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

app.UseEndpoints(endpoint => {
    endpoint.MapRazorPages();
});

app.Run();
