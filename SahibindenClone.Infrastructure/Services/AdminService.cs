using Microsoft.EntityFrameworkCore;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Domain.Enums;
using SahibindenClone.Infrastructure.Context;

namespace SahibindenClone.Infrastructure.Services;

public sealed class AdminService(ApplicationDbContext db) : IAdminService
{
    public async Task<AdminDashboardDto> GetDashboardAsync() => new(
        await db.Users.CountAsync(), await db.Users.CountAsync(u => u.IsActive),
        await db.Adverts.CountAsync(a => a.IsActive),
        await db.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.PendingApproval),
        await db.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.Active),
        await db.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.Rejected),
        await db.Categories.CountAsync(c => c.IsActive));

    public async Task<IReadOnlyList<AdminAdvertDto>> GetAdvertsAsync(AdvertStatus? status)
    {
        var query = db.Adverts.AsNoTracking().Where(a => a.IsActive);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        return await query.OrderByDescending(a => a.CreatedAt)
            .Select(a => new AdminAdvertDto(a.Id, a.Title, a.Description, a.Price,
                a.User.FirstName + " " + a.User.LastName, a.Category.Name, a.City.Name,
                (int)a.Status, StatusName(a.Status), a.CreatedAt,
                a.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()))
            .ToListAsync();
    }

    public async Task<ServiceResult<bool>> SetAdvertStatusAsync(int id, AdvertStatus status, int adminUserId)
    {
        if (status is not (AdvertStatus.Active or AdvertStatus.Rejected))
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yalnızca onaylama veya reddetme işlemi yapılabilir.");
        var advert = await db.Adverts.FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        if (advert is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.Status != AdvertStatus.PendingApproval)
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yalnızca onay bekleyen ilanlar karara bağlanabilir.");
        advert.Status = status;
        advert.UpdatedAt = DateTime.UtcNow;
        AddAudit(adminUserId, status == AdvertStatus.Active ? "advert.approved" : "advert.rejected", "Advert", id, $"İlan {StatusName(status)} olarak işaretlendi.");
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync() => await db.Users.AsNoTracking()
        .OrderByDescending(u => u.CreatedAt)
        .Select(u => new AdminUserDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role,
            u.IsActive, u.CreatedAt, u.Adverts.Count(a => a.IsActive)))
        .ToListAsync();

    public async Task<ServiceResult<bool>> SetUserActiveAsync(int id, bool isActive, int actingAdminId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kullanıcı bulunamadı.");
        if (!isActive && user.Id == actingAdminId)
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Kendi hesabınızı devre dışı bırakamazsınız.");
        if (!isActive && user.Role == "Admin")
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Admin hesapları bu panelden devre dışı bırakılamaz.");
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        AddAudit(actingAdminId, isActive ? "user.activated" : "user.deactivated", "User", id,
            $"{user.Email} hesabı {(isActive ? "etkinleştirildi" : "devre dışı bırakıldı")}.");
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync() => await db.Categories.AsNoTracking()
        .OrderBy(c => c.Name)
        .Select(c => new AdminCategoryDto(c.Id, c.Name, c.ParentId, c.Parent == null ? null : c.Parent.Name,
            c.IsActive, c.Adverts.Count(a => a.IsActive)))
        .ToListAsync();

    public async Task<ServiceResult<int>> CreateCategoryAsync(AdminCategoryUpsertDto dto, int adminUserId)
    {
        var validation = await ValidateCategoryAsync(dto, null);
        if (validation is not null) return ServiceResult<int>.Failure(validation.Value.Error, validation.Value.Message);
        var category = new Category { Name = dto.Name.Trim(), ParentId = dto.ParentId, IsActive = true, CreatedAt = DateTime.UtcNow };
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        AddAudit(adminUserId, "category.created", "Category", category.Id, $"'{category.Name}' kategorisi oluşturuldu.");
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return ServiceResult<int>.Success(category.Id);
    }

    public async Task<ServiceResult<bool>> UpdateCategoryAsync(int id, AdminCategoryUpsertDto dto, int adminUserId)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kategori bulunamadı.");
        var validation = await ValidateCategoryAsync(dto, id);
        if (validation is not null) return ServiceResult<bool>.Failure(validation.Value.Error, validation.Value.Message);
        category.Name = dto.Name.Trim();
        category.ParentId = dto.ParentId;
        category.IsActive = true;
        category.UpdatedAt = DateTime.UtcNow;
        AddAudit(adminUserId, "category.updated", "Category", id, $"Kategori adı '{category.Name}' olarak güncellendi.");
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id, int adminUserId)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (category is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kategori bulunamadı.");
        if (await db.Categories.AnyAsync(c => c.ParentId == id && c.IsActive))
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Önce bu kategoriye bağlı alt kategorileri taşıyın veya silin.");
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        AddAudit(adminUserId, "category.deactivated", "Category", id, $"'{category.Name}' kategorisi silindi.");
        await db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminAuditLogDto>> GetAuditLogsAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 200);
        return await db.AdminAuditLogs.AsNoTracking().Include(log => log.AdminUser)
            .OrderByDescending(log => log.CreatedAt).Take(limit)
            .Select(log => new AdminAuditLogDto(log.Id, log.AdminUserId,
                log.AdminUser.FirstName + " " + log.AdminUser.LastName,
                log.Action, log.TargetType, log.TargetId, log.Details, log.CreatedAt))
            .ToListAsync();
    }

    private void AddAudit(int adminUserId, string action, string targetType, int targetId, string details) =>
        db.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminUserId, Action = action, TargetType = targetType,
            TargetId = targetId, Details = details, CreatedAt = DateTime.UtcNow
        });

    private async Task<(ServiceError Error, string Message)?> ValidateCategoryAsync(AdminCategoryUpsertDto dto, int? id)
    {
        var name = dto.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Id != id && c.IsActive && c.Name.ToLower() == name.ToLower()))
            return (ServiceError.Conflict, "Bu isimde aktif bir kategori zaten var.");
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId == id) return (ServiceError.InvalidOperation, "Kategori kendisinin üst kategorisi olamaz.");
            if (!await db.Categories.AnyAsync(c => c.Id == dto.ParentId && c.IsActive && c.ParentId == null))
                return (ServiceError.InvalidOperation, "Üst kategori olarak yalnızca ana kategoriler seçilebilir.");
            if (id.HasValue && await db.Categories.AnyAsync(c => c.ParentId == id && c.IsActive))
                return (ServiceError.InvalidOperation, "Alt kategorisi bulunan bir ana kategori alt kategoriye dönüştürülemez.");
        }
        return null;
    }

    private static string StatusName(AdvertStatus status) => status switch
    {
        AdvertStatus.Active => "Aktif", AdvertStatus.Sold => "Satıldı", AdvertStatus.Expired => "Süresi Doldu",
        AdvertStatus.Rejected => "Reddedildi", _ => "Onay Bekliyor"
    };
}
