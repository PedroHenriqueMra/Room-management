using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalApi.Endpoints;
using Services.Configuration.DI;
// using MinimalApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure DIs
builder.Services.AddAllDependencies();

builder.Services.AddDbContext<DbContextModel>(
    options => options.UseSqlite("Data Source=adjisajd.db"));

// identity:
builder.Services.AddDefaultIdentity<IdentityUser>(opt => opt.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DbContextModel>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(opt => {
    // provisorio: desabilitar confirmação de email
    opt.SignIn.RequireConfirmedAccount = false;
    opt.SignIn.RequireConfirmedEmail = false;
    opt.SignIn.RequireConfirmedPhoneNumber = false;

    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 8;

    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(opt => {
    opt.LoginPath = "/auth/login";
    opt.Cookie.Name = "auth_cookie";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    opt.SlidingExpiration = true;
    opt.Cookie.HttpOnly = true;
});

builder.Services.AddRazorPages(opt => {
    // desabilita o token de formulario obrigatorio
    // opt.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

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
