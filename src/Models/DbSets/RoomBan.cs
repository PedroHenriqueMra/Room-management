using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MinimalApi.DbSet.Models;

public class RoomBan
{ 
    [Key]
    public int BanId { get; set; }
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; }
    public Guid RoomId { get; set; }
    public BanType BanType { get; set; } = default;
    public DateTime BanStart { get; set; }
    public DateTime BanEnd { get; set; }
    public string? Reason { get; set; }
    public bool Status { get; set; } = true;
}

public enum BanType
{
    Temporary,
    Permanent
}
