using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MinimalApi.DbSet.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using static LoginModel;

[Authorize]
public class CreateRoomModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly DbContextModel _context;
    private readonly IServicesCreateRoom _serviceCreate;

    public CreateRoomModel(ILogger<RegisterModel> logger, DbContextModel context, IServicesCreateRoom serviceCreate)
    {
        _context = context;
        _logger = logger;
        _serviceCreate = serviceCreate;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [StringLength(150)]
        public string Description { get; set; }
        public bool IsPrivate { get; set; } = default;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var roomsContext = _context.Room.FirstOrDefault(r => r.Name == Input.Name);
        if (roomsContext != null)
        {
            ViewData["NameFail"] = true;
            return Page();
        }

        if (!User.Identity.IsAuthenticated)
        {
            return NotFound("You aren'n authenticated!!");
        }
        var claimEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        // create a new room
        var adm = await _context.User.FirstOrDefaultAsync(u => u.Email == claimEmail);
        if (adm == null)
        {
            // usuario nao encontrado
            return NotFound("User not found");
        }

        var data = new CreateRoomRequest(){
            IdUser = adm.Id,
            Name = Input.Name,
            Description = Input.Description,
            IsPrivate = Input.IsPrivate
        };

        var result = await _serviceCreate.CreateRoomAsync(data);
        if (result is BadRequestObjectResult)
        {
            _logger.LogError($"An error ocurred when created the room. Room name {Input.Name}.!!");
            return Page();
        }

        var roomUrl = await _context.Room.FirstOrDefaultAsync(r => r.Name == Input.Name);
        if (roomUrl != null)
        {
            return Redirect($"http://localhost:5229/rooms/{roomUrl.Id}");
        }

        return Page();
    }
}
