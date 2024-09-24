using System.ComponentModel.DataAnnotations;

namespace ModelTables;

public class Message
{
    public int Id { get; set; }
    [Required]
    [StringLength(1000)]
    public string Content { get; set; }
    public bool IsAdm { get; set; } = default;

    // muitos para um:
    [Required]
    public User User { get; set; }
    public int UserId { get; set; }

    // muitos para um:
    [Required]
    public Room Room { get; set; }
    public Guid RoomId { get; set; }
}

