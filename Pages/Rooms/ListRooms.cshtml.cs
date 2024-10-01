using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModelTables;

[Authorize]
public class ListRoomModel : PageModel
{
    public ILogger<ListRoomModel> _logger;
    public DbContextModel _context;

    public ListRoomModel(ILogger<ListRoomModel> logger, DbContextModel context)
    {
        _context = context;
        _logger = logger;
    }

    public List<Room> Rooms { get; set; } = new List<Room>();
    public User Owner { get; set; } = new User();

    public async Task OnGetAsync()
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            Owner = user;
        }

        var rooms = await _context.Room.Include(r => r.Adm).ToListAsync();
        if (rooms.Count != 0)
        {
            foreach(var r in rooms)
            {
                Rooms.Add(r);
            }
        }
    }
}