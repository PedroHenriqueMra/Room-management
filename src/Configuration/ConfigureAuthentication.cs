using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace MinimalApi.Services.Authentication;

public static class ConfigureAuth
{
    
    public static void ConfigureIdentity(this IServiceCollection service)
    {
        // identity:
        service.AddDefaultIdentity<IdentityUser>(opt => opt.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<DbContextModel>()
            .AddDefaultTokenProviders();

        service.Configure<IdentityOptions>(opt => {
            // provisional: disable email confirmer
            opt.SignIn.RequireConfirmedAccount = false;
            opt.SignIn.RequireConfirmedEmail = false;
            opt.SignIn.RequireConfirmedPhoneNumber = false;

            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 8;

            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.AllowedForNewUsers = true;
        });
    }

    public static void ConfigureCookie(this IServiceCollection service)
    {
        service.AddAuthentication(opt => {
            opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(opt => {
                opt.LoginPath = "/auth/login";
                opt.Cookie.Name = "auth_cookie";
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                opt.SlidingExpiration = true;
                opt.Cookie.HttpOnly = true;
        });
    }
}
