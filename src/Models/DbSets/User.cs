using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MinimalApi.DbSet.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; }
    [Required]
    [EmailAddress]
    [StringLength(260)]
    public string Email { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]")]
    public string Password { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

    // um para muitos:
    [JsonIgnore]
    public ICollection<Room>? Rooms { get; set; } = new List<Room>();
    public List<string>? RoomsNames { get; set; } = new List<string>();

    // um para muitos:
    public ICollection<Message>? Messages { get; set; } = new List<Message>();
}
