using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginModel(ILogger<LoginModel> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
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
        public string Remember { get; set; }
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

        bool remember = Input.Remember != null ? true : false;
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, remember, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User is logged in now");
            return LocalRedirect("~/");
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out");
            return Redirect("~/");
        }
        if (result.IsNotAllowed)
        {
            _logger.LogError("Confirmation is required for this account");
            // redirect para a pagina de confirmação
            return Page();
        }

        return RedirectToAction("/");
    }
}
