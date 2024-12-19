using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MinimalApi.DbSet.Models;
using NuGet.Protocol;
using ZstdSharp;

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
    public struct BannedUsersDict
    {
        public int BanId { get; set; }
        public BanType BanType { get; set; }
        public string BanReason { get; set; }
        public DateTime BanEnd { get; set; }
        public string UserName { get; set; }
    }
    public class RoomViewModel
    {
        public Room Room { get; set; } = new Room();
        public List<User>? UsersInRoom { get; set; }
        public List<BannedUsersDict> BannedUsers { get; set; }
    }
    public RoomViewModel RoomView { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid roomId)
    {
       if (roomId == Guid.Empty)
       {
            return Redirect("/home");
       }

        var user = await GetUserbyClaimsAsync();
        if (user == null)
        {
            return Redirect("/auth/login");
        }

        var room = await _context.Room.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null)
        {
            return Redirect("/home");
        }

        if (room.AdmId != user.Id)
        {
            return Redirect("/home");
        }
        var bannedUsers = await _context.RoomBan.Include(u => u.User).Where(b => b.RoomId == room.Id).ToListAsync();
        List<BannedUsersDict> dictBannedUsers = new List<BannedUsersDict>();
        foreach (var bans in bannedUsers)
        {
            dictBannedUsers.Add(new BannedUsersDict()
            {
                BanId = bans.BanId,
                BanType = bans.BanType,
                BanReason = bans.Reason ?? "There is not reason...",
                BanEnd = bans.BanEnd,
                UserName = bans.User.Name
            });
        }

        // Room = room;
        RoomView = new RoomViewModel()
        {
            Room = room,
            UsersInRoom = room.Users.Where(u => u.Id != room.AdmId).ToList(),
            BannedUsers = dictBannedUsers
        };

        return Page();
    }

    // private Task<Dictionary<int, string>> GetUsersInRoomAsync(Room room)
    // {

    // }

    private async Task<User>? GetUserbyClaimsAsync()
    {
        var claimsId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claimsId == null)
        {
            return null;
        }

        var queryUser = await _context.User.FindAsync(Int32.Parse(claimsId));
        if (queryUser != null)
        {
            return queryUser;
        }

        return null;
    }
}
