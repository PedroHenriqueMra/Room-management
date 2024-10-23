using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class IndexModel :PageModel
{
    public string? Id { get; set; }
    public string? Email { get; set; }

    public void OnGet()
    {
        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "No authenticated";
        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "No authenticated";
    }

    public IActionResult OnPost()
    {
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (claimEmail != null)
        {
            return RedirectToPage($"user/manager/{claimEmail}");
        }

        return RedirectToPage("home");
    }
}
