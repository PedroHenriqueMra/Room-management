using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.ServicesUser.Change;

public class BaseModelChangePassword : PageModel
{
    private readonly ILogger<BaseModelChangePassword> _logger;
    private readonly IUserManageServices _userManagerService;
    private readonly IGetMessageError _getMessageError;
    public BaseModelChangePassword(ILogger<BaseModelChangePassword> logger, IUserManageServices userManagerService, IGetMessageError getMessageError)
    {
        _logger = logger;
        _userManagerService = userManagerService;
        _getMessageError = getMessageError;
    }

    public int Id;
    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id == null)
        {
            _logger.LogWarning("The user id is null!");
            return Redirect("/home");
        }

        Id = id;
        _logger.LogInformation($"change password id={id}");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] string password, [FromForm] string confirmPassword, [FromForm] int id)
    {
        if (id == null)
        {
            _logger.LogWarning("The user id is null!");
            return Redirect("/home");
        }

        if (confirmPassword != password)
        {
            _logger.LogWarning($"The confirm-password: {confirmPassword} don't matches password: {password}");
            TempData["ErrorConfirmPassword"] = "Error. Confirm password is not valid.";
            return Page();
        }

        var result = await _userManagerService.PasswordChangeAsync(id, password);
        if (result is IStatusCodeHttpResult status && status.StatusCode > 299)
        {
            string msg = _getMessageError.GetMessage(result, "Value", '=', '}');

            _logger.LogWarning($"An error ocurred while performed the service. Message: {msg}");
            TempData["ErrorPasswordForm"] = msg;
            return Page();
        }

        _logger.LogInformation("The password was changed successfully!");
        return Page();
    }
}
