using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class RoomModel : PageModel
{
    public ILogger<RoomModel> _logger;

    public RoomModel(ILogger<RoomModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        
    }
}