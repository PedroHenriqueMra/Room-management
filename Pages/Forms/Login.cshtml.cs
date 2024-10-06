using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly DbContextModel _context;
    private readonly IHttpContextAccessor _httpContext;
    public LoginModel(ILogger<LoginModel> logger, DbContextModel context, IHttpContextAccessor httpContext)
    {
        _logger = logger;
        _context = context;
        _httpContext = httpContext;
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
        public bool Remember { get; set; } = default;
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

        var confereUser = await _context.User.FirstOrDefaultAsync(u => u.Email == Input.Email);
        if (confereUser == null)
        {
            _logger.LogError("Error in email");
            return Page();
            // return (IActionResult)Results.BadRequest("Error in email");
        }
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(Input.Password, confereUser.Password);
        if (!passwordMatch)
        {
            _logger.LogError("Error in password");
            return Page();
            // return (IActionResult)Results.BadRequest("Error in password");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, Input.Email),
            new Claim(ClaimTypes.NameIdentifier, confereUser.Id.ToString())
        };
        try
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await _httpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { IsPersistent = Input.Remember });

            _logger.LogInformation("Authenticated user");
            // depois fazer uma tela para sucesso de autenticação
            return RedirectToAction("/");
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Exeption error: {ex}");
            return (IActionResult)Results.BadRequest("An error occurred during login.");
        }
    }
}
