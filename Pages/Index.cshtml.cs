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
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!String.IsNullOrEmpty(claimEmail))
        {
            return Redirect($"http://localhost:5229/user/manager/{claimEmail}");
        }

        Email = claimEmail ?? "No authenticated";
        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "No authenticated";
        return Redirect("http://localhost:5229/home");
    }
}
