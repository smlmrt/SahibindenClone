using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Interfaces;

/// <summary>
/// Admin denetim logları için özel repository arayüzü.
/// </summary>
public interface IAdminAuditLogRepository : IGenericRepository<AdminAuditLog>
{
    /// <summary>
    /// En son gerçekleşen denetim loglarını ilişkili admin kullanıcı bilgisiyle birlikte getirir.
    /// </summary>
    Task<IReadOnlyList<AdminAuditLog>> GetRecentLogsWithUserAsync(int limit);
}
