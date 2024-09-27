using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModelTables;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using static LoginModel;

[Authorize]
public class CreateRoomModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly DbContextModel _context;

    public CreateRoomModel(ILogger<RegisterModel> logger, DbContextModel context)
    {
        _context = context;
        _logger = logger;
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
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var roomsContext = await _context.Room.ToListAsync();
        foreach(var r in roomsContext)
        {
            if (r.Name == Input.Name)
            {
                ViewData["NameFail"] = true;
                return Page();
            }
        }

        // create a new room
        var adm = await _context.User.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (adm == null)
        {
            // usuario nao encontrado
            return NotFound("User not found");
        }

        var data = new {
            IdUser = adm.Id,
            Name = Input.Name,
            Description = Input.Description
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = new HttpClient() { BaseAddress = new Uri("http://localhost:5229") };
        var response = await client.PostAsync("/new/room", content);

        if (!response.IsSuccessStatusCode)
        {
            // erro!!!! (adicionar comunicação do erro com o cliente)
            return Page();
        }

        var roomUrl = await _context.Room.FirstOrDefaultAsync(r => r.Name == Input.Name);
        if (roomUrl != null)
        {
            return RedirectToAction($"/rooms/{roomUrl.Id}");
        }

        return Page();
    }
}
