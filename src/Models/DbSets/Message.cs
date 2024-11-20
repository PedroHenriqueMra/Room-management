using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MinimalApi.DbSet.Models;

public class Message
{
    public int Id { get; set; }
    [Required]
    [StringLength(200)]
    public string Content { get; set; }

    // muitos para um:
    [Required]
    [JsonIgnore]
    public User User { get; set; }
    public int UserId { get; set; }

    // muitos para um:
    [Required]
    [JsonIgnore]
    public Room Room { get; set; }
    public Guid RoomId { get; set; }
}
