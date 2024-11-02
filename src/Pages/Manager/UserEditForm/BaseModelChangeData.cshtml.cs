using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalApi.DbSet.Models;
using Org.BouncyCastle.Security;

[Authorize]
public class BaseModelChangeData : PageModel
{
    private readonly ILogger<BaseModelChangeData> _logger;
    private readonly DbContextModel _context;
    private readonly IUserManageServices _userChangeService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IGetMessageError _getMessageError;
    public BaseModelChangeData(ILogger<BaseModelChangeData> logger, DbContextModel context, IUserManageServices userChangeService, IHttpContextAccessor httpContext, IGetMessageError getMessageError)
    {
        _logger = logger;
        _context = context;
        _userChangeService = userChangeService;
        _httpContext = httpContext;
        _getMessageError = getMessageError;
    }

    public int Id;
    public User? User;
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            return NotFound($"The user which id is {id} not found!");
        }

        _logger.LogInformation("Change email/name");
        return Page();
    }

    // Confirm password form:
    public async Task<IActionResult> OnPostAsync([FromForm] string password, int id)
    {
        if (String.IsNullOrEmpty(password))
        {
            return Page();
        }

        var user = await _context.User.FindAsync(id);
        Id = user.Id;
        User = user;
        if (user == null)
        {
            return NotFound($"The user which id is {id} not found!");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            _logger.LogInformation("password is incorrect!!");
            TempData["IncorrectPassword"] = true;
            return Page();
        }

        _logger.LogInformation("password is correct!!");
        ViewData["TemplateForm"] = "_FormChangeNameEmailPartial.cshtml";
        return Page();
    }

    // Change name/email form:
    public async Task<IActionResult> OnPostChangeAsync(int id, [FromForm] string? email, [FromForm] string? name)
    {
        Id = id;
        var user = await _context.User.FindAsync(id);
        User = user;

        if (email != null && email != user.Email)
        {
            var result = await _userChangeService.EmailChangeAsync(id, email);

            if (result is IStatusCodeHttpResult status && status.StatusCode > 299)
            {
                string msg = _getMessageError.GetMessage(result, "Value", '=', '}');

                _logger.LogError($"An error ocurred when change email. Message: {msg}");
                TempData["ErrorMessageEmail"] = msg;
            }
            else
            {
                _logger.LogInformation("Success to change email!!");
                TempData["SuccessEmail"] = "Email was changed successffuly!";
            }
        }

        if (name != null && name != user.Name)
        {
            var result = await _userChangeService.NameChangeAsync(id, name);
            if (result is IStatusCodeHttpResult response && response.StatusCode > 299)
            {
                string msg = _getMessageError.GetMessage(result, "Value", '=', '}');
                
                _logger.LogError($"An error ocurred when change name. Message: {msg}");
                TempData["ErrorMessageName"] = msg;
            }
            else
            {
                _logger.LogInformation("Success to change email!!");
                TempData["SuccessName"] = "Name was changed successffuly!";
            }
        }

        ViewData["TemplateForm"] = "_FormChangeNameEmailPartial.cshtml";
        return Page();
    }
}
