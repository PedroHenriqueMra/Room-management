using System.ComponentModel.DataAnnotations;

namespace MinimalApi.DbSet.Models;

public class Message
{
    public int Id { get; set; }
    [Required]
    [StringLength(200)]
    public string Content { get; set; }

    // muitos para um:
    [Required]
    public User User { get; set; }
    public int UserId { get; set; }

    // muitos para um:
    [Required]
    public Room Room { get; set; }
    public Guid RoomId { get; set; }
}

public class MessageRequest
{
    [Required]
    public string Content { get; set; }
    [Required]
    public Guid IdRoom { get; set; }
    [Required]
    public int IdUser { get; set; }
}
