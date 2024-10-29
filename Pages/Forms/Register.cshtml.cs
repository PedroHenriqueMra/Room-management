using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MinimalApi.DbSet.Models;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;
using MinimalApi.Services.Utils.RegularExpression;
using Microsoft.AspNetCore.Http.HttpResults;
using NuGet.Common;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IUserServices _userServices;
    private readonly DbContextModel _context;
    private readonly IGetMessageError _getMessageError;
    public RegisterModel(ILogger<RegisterModel> logger, IHttpContextAccessor httpContext, IUserServices userServices, DbContextModel context, IGetMessageError getMessageError)
    {
        _logger = logger;
        _httpContext = httpContext;
        _userServices = userServices;
        _context = context;
        _getMessageError = getMessageError;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "the name must have be between 3 and 100 characters")]
        [DataType(DataType.Text)]
        [Display(Name = "User name")]
        public string Name { get; set; }
        [Required]
        [StringLength(260)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Your Email")]
        public string Email { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "the password must have be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,100}$", ErrorMessage = "Inválid Characters!!")]
        [Display(Name = "Your password")]
        public string Password { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Imcomplete data or input error");
            return Page();
        }

        var newUser = new User()
        {
            Name = Input.Name,
            Email = Input.Email,
            Password = Input.Password
        };
        try
        {
            var response = await _userServices.UserCreateAsync(newUser);
            if (response is IStatusCodeHttpResult status && status.StatusCode > 299)
            {
                string msg = _getMessageError.GetMessage(response, "Value", '=', '}');
                TempData["ErrorMessage"] = msg;

                _logger.LogWarning($"An error ocurred to register. Message error: {msg}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error identified: {ex}");
            return (IActionResult)Results.BadRequest("Excecption found");
        }

        _logger.LogInformation("User created");
        var userId = await _context.User.FirstAsync(u => u.Email == newUser.Email);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, newUser.Email),
            new Claim(ClaimTypes.NameIdentifier, userId.Id.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await _httpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { IsPersistent = false });

        return Redirect("/home");
    }
}
