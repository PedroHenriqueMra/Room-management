using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public string? Id;
    public string? Email;

    public void OnGet()
    {
        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "No authenticated";
        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "No authenticated";
    }

    public IActionResult OnPost()
    {
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claimId != null)
        {
            return Redirect($"/user/manager/{claimId}");
        }

        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        Email = claimEmail ?? "No authenticated";
        Id = "No authenticated";
        return Redirect("/home");
    }
}
