using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MinimalApi.DbSet.Models;

[Authorize]
public class BaseModelChangeData : PageModel
{
    private readonly ILogger<BaseModelChangeData> _logger;
    private readonly DbContextModel _context;
    public BaseModelChangeData(ILogger<BaseModelChangeData> logger, DbContextModel context)
    {
        _logger = logger;
        _context = context;
    }

    public int Id;
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            return NotFound($"The user which id is {id} not found!");
        }
        Id = id;

        if (Request.Path.ToString().Contains("password"))
        {
            _logger.LogInformation("Change password");
            return Page();
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
        if (user == null)
        {
            return NotFound($"The user which id is {id} not found!");
        }
        Id = user.Id;

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            _logger.LogInformation("password is correct!!");
            ViewData["TemplateForm"] = "_FormChangeNameEmailPartial";
            return Page();
        }
        _logger.LogInformation("password is incorrect!!");
        return Page();
    }

    // Change name/email form:
    public async Task<IActionResult> OnPostChangeAsync(int id, [FromForm] string? email, [FromForm] string? name)
    {
        
    }
}
