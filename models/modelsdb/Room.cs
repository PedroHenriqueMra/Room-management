using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ModelTables;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    // um para muitos:
    [Required]
    public User Adm { get; set; }
    [Required]
    public int AdmId { get; set; }
    [StringLength(150)]
    public string Description { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

    // um para muitos:
    public ICollection<Message>? Messages { get; set; } = new List<Message>();

    // um para muitos:
    [JsonIgnore]
    public ICollection<User>? Users { get; set; } = new List<User>();
    public List<string>? UsersNames { get; set; } = new List<string>();
}

public class RoomRequest
{
    public int IdUser { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
