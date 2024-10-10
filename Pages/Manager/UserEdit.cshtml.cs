using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;

[Authorize]
public class UserEditModel : PageModel
{
    private readonly ILogger<UserEditModel> _logger;
    private readonly DbContextModel _context;

    public UserEditModel(ILogger<UserEditModel> logger, DbContextModel context)
    {
        _logger = logger;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public class InputModel
    {
        [Display(Name = "Name")]
        [StringLength(50, MinimumLength = 3)]
        public string? Name { get; set; }
        [Display(Name = "Email")]
        [EmailAddress]
        [StringLength(260)]
        public string? Email { get; set; }
        [Display(Name = "password")]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]")]
        public string? Password { get; set; }
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Erro. Confirme a senha corretamente!")]
        public string? ConfirmPassword { get; set; }
    }
    public User UserManager { get; set; }
    public string What { get; set; }
    private static readonly List<string> AllowedFields = new List<string> { "name", "email", "password" };
    
    // post
    [ValidateAntiForgeryToken()]
    public async Task<IActionResult> OnPostAsync([FromForm] string? type, string? email)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Error data");
            return Page();
        }
        var validType = IsValidFieldType(type);

        if (!validType)
        {
            _logger.LogWarning("Request type not informed or invalid!");
            return Page();
        }
        email = String.IsNullOrEmpty(email) ? null : email ;

        if (email == null)
        {
            _logger.LogWarning("User email error, This email is null or empty");
            return Redirect("http://localhost:5229/auth/login");
        }

        if (!IsAuthenticated(email))
        {
            _logger.LogWarning("Unauthenticated user");
            return Redirect("http://localhost:5229/auth/login");
        }

        if (type == "email")
        {
            if (Input.Email == null)
            {
                _logger.LogWarning("Error email null");
                return Page();
            }

            var queryDataBase = await _context.User.FirstOrDefaultAsync(u => u.Email == Input.Email);

            if (queryDataBase != null)
            {
                _logger.LogWarning($"This email {Input.Email} already existes!!");
                return Page();
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            try
            {
                user.Email = Input.Email;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Email updated for user: {email}");
                return Redirect($"http://localhost:5229/user/manager/{email}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogCritical($"An error ocurred while inserting data into the database!, message: {ex}");
                return Page();
            }
        }
        else if (type == "name")
        {
            if (Input.Name == null)
            {
                _logger.LogCritical("Error name null");
                return Page();
            }

            var queryDataBase = await _context.User.FirstOrDefaultAsync(u => u.Name == Input.Name);

            if (queryDataBase != null)
            {
                _logger.LogWarning($"This name {Input.Name} already existes!!");
                return Page();
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            try
            {
                user.Name = Input.Name;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Name updated for user: {email}");
                return Redirect($"http://localhost:5229/user/manager/{email}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogCritical($"An error ocurred while inserting data into the database!, message: {ex}");
                return Page();
            }
        }
        else // change password
        {
            if (Input.Password == null || Input .ConfirmPassword == null)
            {
                _logger.LogWarning("Error, Password/confirmpassword null");
                return Page();
            }

            if (!Input.Password.Equals(Input.ConfirmPassword))
            {
                _logger.LogWarning("Error, Password and PasswordConfirm are different!");
                return Page();
            }

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password);
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if(user == null)
            {
                _logger.LogWarning($"User not found in system!. Email {email} not found!");
                return Redirect($"http://localhost:5229/auth/login");
            }

            try
            {
                user.Password = hashPassword;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Password updated for user: {email}");
                return Redirect($"http://localhost:5229/user/manager/{email}");

            }
            catch (DbUpdateException ex)
            {
                _logger.LogCritical($"An error ocurred while inserting data into the database!, message: {ex}");
                return Page();
            }
        }
    }

    public async Task<IActionResult> OnGetAsync(string what, string email)
    {
        if (String.IsNullOrEmpty(what) || String.IsNullOrEmpty(email))
        {
            _logger.LogCritical("Url data is empty");
            return Redirect("http://localhost:5229/home");
        }

        if (!AllowedFields.Contains(what))
        {
            _logger.LogCritical("Url isn't allowed");
            return Redirect("http://localhost:5229/home");
        }

        if (what == null)
        {
            _logger.LogCritical($"Invalid parameter in url");
            return Redirect("http://localhost:5229/home");
        }

        if (!IsAuthenticated(email))
        {
            _logger.LogCritical("User isn't authenticate");
            return Redirect("http://localhost:5229/home");
        }

        try
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogCritical("User not found in system");
                return Redirect("http://localhost:5229/home");
            }

            UserManager = user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Database error: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }

        What = what;
        ViewData["Email"] = email;
        ViewData["Title"] = $"Editing {what}";
        return Page();
    }

    private bool IsValidFieldType(string type) => AllowedFields.Contains(type);

    private bool IsAuthenticated(string email)
    {
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (claimEmail != email)
        {
            return false;
        }

        return true;
    }
}
