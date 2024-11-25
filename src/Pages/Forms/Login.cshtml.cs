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
using MinimalApi.Services.Utils.RegularExpression.User;
using Microsoft.AspNetCore.Http.HttpResults;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly DbContextModel _context;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IUserServices _userService;
    private readonly IGetPropertyAnonymous _getMessageError;
    public LoginModel(ILogger<LoginModel> logger, DbContextModel context, IHttpContextAccessor httpContext, IUserServices userService, IGetPropertyAnonymous getMessageError)
    {
        _logger = logger;
        _context = context;
        _httpContext = httpContext;
        _userService = userService;
        _getMessageError = getMessageError;
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
        [StringLength(50, MinimumLength = 8, ErrorMessage = "the password must have be between 8 and 100 characters")]
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

        var result = await _userService.UserLoginAsync(Input.Password, Input.Email);
        if (result is IStatusCodeHttpResult status && status.StatusCode > 299)
        {
            string msg = _getMessageError.GetMessage(result, "Value", '=', '}');
            TempData["MessageError"] = msg;

            _logger.LogInformation($"Something went wrong in authentication for user {Input.Email}. Messag: {msg}");
            return Page();
        }

        var user = await _context.User.FirstAsync(u => u.Email == Input.Email);
        if (user != null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
            try
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await _httpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { IsPersistent = Input.Remember });

                _logger.LogInformation("Authenticated user");
                return Redirect("/home");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exeption error: {ex}");
                return (IActionResult)Results.BadRequest("An error occurred during login.");
            }
        }

        _logger.LogDebug($"An error ocurred in user query!. User: {Input.Email}");
        return Page();
    }
}
