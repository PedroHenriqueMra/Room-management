using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Google.Protobuf.WellKnownTypes;
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
using Newtonsoft.Json;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    public RegisterModel(ILogger<RegisterModel> logger, UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // post request to create a new user:
        var data = new
        {
            Name = Input.Name,
            Email = Input.Email,
            Password = Input.Password
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = new HttpClient() { BaseAddress = new Uri("http://localhost:5229") };
        var response = await client.PostAsync("/new", content);

        if (!response.IsSuccessStatusCode)
        {
            return Content($"Requisição falha!!: {response.StatusCode}");
        }

        // creation of the entity
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
            return Page();
        }

        _logger.LogInformation("User created successfully");
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/index");
    }
}
