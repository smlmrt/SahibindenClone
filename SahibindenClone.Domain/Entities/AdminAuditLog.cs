namespace SahibindenClone.Domain.Entities;

public sealed class AdminAuditLog : BaseEntity
{
    public int AdminUserId { get; set; }
    public virtual User AdminUser { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Details { get; set; } = string.Empty;
}
