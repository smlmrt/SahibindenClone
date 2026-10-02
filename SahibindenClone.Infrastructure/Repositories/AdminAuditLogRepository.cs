using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Repositories;

public class AdminAuditLogRepository : GenericRepository<AdminAuditLog>, IAdminAuditLogRepository
{
    public AdminAuditLogRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AdminAuditLog>> GetRecentLogsWithUserAsync(int limit)
    {
        return await _context.AdminAuditLogs
            .AsNoTracking()
            .Include(log => log.AdminUser)
            .OrderByDescending(log => log.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
