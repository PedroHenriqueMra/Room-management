using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.DbSet.Models;
using NuGet.Protocol;

[Authorize]
public class EditRoomModel : PageModel
{
    private readonly ILogger<EditRoomModel> _logger;
    private readonly DbContextModel _context;
    // private readonly IExitRoomService _exitRoomService;

    public EditRoomModel(ILogger<EditRoomModel> logger, DbContextModel context)
    {
        _context = context;
        _logger = logger;
    }

    // variables to render
    public Room Room { get; set; } = new Room();
    public List<User> Users { get; set; } = new List<User>();

    public Task<IActionResult> OnGetAsync(Guid roomId)
    {
        
    }

    private Task<Dictionary<int, string>> GetUsersInRoomAsync(Room room)
    {

    }
}
