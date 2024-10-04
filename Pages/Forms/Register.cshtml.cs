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
using ModelTables;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly HttpContext _context;
    private readonly HttpClient _client;
    public RegisterModel(ILogger<RegisterModel> logger, HttpContext context, HttpClient client)
    {
        _logger = logger;
        _context = context;
        _client = client;
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
        [StringLength(100, MinimumLength = 8, ErrorMessage = "the password must have be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,100}$", ErrorMessage = "Inválid Characters!!")]
        [Display(Name = "Your password")]
        public string Password { get; set; }
    }

    private string UserId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Imcomplete data or input error");
            return Page();
        }

        var DataRegister = new {
            Name = Input.Name,
            Email = Input.Email,
            Password = Input.Password
        };
        var json = JsonConvert.SerializeObject(DataRegister);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _client.PostAsync("http://localhost:5229/new", content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request to '/new' error");
                return (IActionResult)Results.BadRequest("Request Error");
            }

            var url = response.Headers.Location;
            if (url == null)
            {
                _logger.LogError("Id not received");
                return (IActionResult)Results.BadRequest();
            }

            UserId = url.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error identified: {ex}");
            return (IActionResult)Results.BadRequest("Excecption found");
        }
        _logger.LogInformation("User created");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, Input.Email),
            new Claim(ClaimTypes.NameIdentifier, UserId)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await _context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
 
        return RedirectToAction("~/rooms");
    }
}
