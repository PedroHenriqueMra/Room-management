using System.ComponentModel.DataAnnotations;

namespace MinimalApi.DbSet.Models;

public class ChatGroup
{
    [Key]
    public Guid ChatId { get; set; }
    public Guid RoomId { get; set; }
    public List<string> ConnectionIds { get; set; } = new List<string>();
}
