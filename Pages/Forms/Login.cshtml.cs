using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginModel(ILogger<LoginModel> logger,SignInManager<IdentityUser> signInManager,UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
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
        [Required]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Erro. Confirme a senha corretamente!")]
        public string ConfirmPassword { get; set; }
        public bool Remember { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.ConfirmPassword != Input.Password)
        {
            ViewData["FailConfirm"] = true;
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            _logger.LogError("Claim not found");
            return (IActionResult)Results.BadRequest("User not found!!");
        }
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, Input.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        try
        {
            await _signInManager.SignInWithClaimsAsync(user, Input.Remember, claims);

            _logger.LogInformation("Authenticated user");
            // depois fazer uma tela para sucesso de autenticação
            return RedirectToAction("~/");
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Exeption error: {ex}");
            return (IActionResult)Results.BadRequest("An error occurred during login.");
        }
    }
}
