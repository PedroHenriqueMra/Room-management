using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServicesApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContextModel>(
    options => options.UseSqlite("Data Source=projectapi.db"));

builder.Services.AddHttpClient();

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

builder.Services.ConfigureApplicationCookie(opt => {
    opt.Cookie.HttpOnly = true;
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    opt.LoginPath = new PathString("/login");
    opt.AccessDeniedPath = new PathString("/");
});

builder.Services.AddRazorPages(opt => {
    // desabilita o token de formulario obrigatorio
    opt.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

var app = builder.Build();

// api service
app.RoutesEndPoints();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoint => {
    endpoint.MapRazorPages();
});

app.Run();
