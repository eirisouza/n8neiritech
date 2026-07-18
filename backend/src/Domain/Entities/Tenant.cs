using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Plan { get; set; }
    public DateTime? PlanExpiresAt { get; set; }
    public ICollection<Store> Stores { get; set; } = new List<Store>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
