using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModelTables;

[Authorize]
public class RoomModel : PageModel
{
    public ILogger<RoomModel> _logger;
    public DbContextModel _context;

    public RoomModel(ILogger<RoomModel> logger, DbContextModel context)
    {
        _context = context;
        _logger = logger;
    }

    public List<Room> Rooms { get; set; } = new List<Room>();
    public User Owner { get; set; } = new ModelTables.User();

    public async Task OnGetAsync()
    {
        var adm = await _context.User.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (adm != null)
        {
            Owner = adm;
        }

        var rooms = await _context.Room.ToListAsync();
        if (rooms.Count != 0)
        {
            foreach(var r in rooms)
            {
                Rooms.Add(r);
            }
        }
    }
}