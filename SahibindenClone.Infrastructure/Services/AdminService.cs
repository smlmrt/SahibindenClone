using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Helpers;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Infrastructure.Services;

public sealed class AdminService(IUnitOfWork uow) : IAdminService
{
    public async Task<AdminDashboardDto> GetDashboardAsync() => new(
        await uow.Users.CountAsync(),
        await uow.Users.CountAsync(u => u.IsActive),
        await uow.Adverts.CountAsync(a => a.IsActive),
        await uow.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.PendingApproval),
        await uow.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.Active),
        await uow.Adverts.CountAsync(a => a.IsActive && a.Status == AdvertStatus.Rejected),
        await uow.Categories.CountAsync(c => c.IsActive));

    public async Task<IReadOnlyList<AdminAdvertDto>> GetAdvertsAsync(AdvertStatus? status)
    {
        var adverts = await uow.Adverts.GetAdvertsForAdminAsync(status);
        return adverts.Select(a => new AdminAdvertDto(
            a.Id,
            a.Title,
            a.Description,
            a.Price,
            a.User is not null ? $"{a.User.FirstName} {a.User.LastName}" : string.Empty,
            a.Category?.Name ?? string.Empty,
            a.City?.Name ?? string.Empty,
            (int)a.Status,
            StatusNameHelper.AdvertStatusName(a.Status),
            a.CreatedAt,
            a.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()))
        .ToList();
    }

    public async Task<ServiceResult<bool>> SetAdvertStatusAsync(int id, AdvertStatus status, int adminUserId)
    {
        if (status is not (AdvertStatus.Active or AdvertStatus.Rejected))
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yalnızca onaylama veya reddetme işlemi yapılabilir.");
        var advert = await uow.Adverts.GetByIdAsync(id);
        if (advert is null || !advert.IsActive) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.Status != AdvertStatus.PendingApproval)
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Yalnızca onay bekleyen ilanlar karara bağlanabilir.");
        advert.Status = status;
        advert.UpdatedAt = DateTime.UtcNow;
        uow.Adverts.Update(advert);
        await AddAuditAsync(adminUserId, status == AdvertStatus.Active ? "advert.approved" : "advert.rejected", "Advert", id, $"İlan {StatusNameHelper.AdvertStatusName(status)} olarak işaretlendi.");
        await uow.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync()
    {
        var users = await uow.Users.GetAllUsersWithAdvertsAsync();
        return users.Select(u => new AdminUserDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Role,
            u.IsActive,
            u.CreatedAt,
            u.Adverts.Count(a => a.IsActive)))
        .ToList();
    }

    public async Task<ServiceResult<bool>> SetUserActiveAsync(int id, bool isActive, int actingAdminId)
    {
        var user = await uow.Users.GetByIdAsync(id);
        if (user is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kullanıcı bulunamadı.");
        if (!isActive && user.Id == actingAdminId)
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Kendi hesabınızı devre dışı bırakamazsınız.");
        if (!isActive && user.Role == "Admin")
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Admin hesapları bu panelden devre dışı bırakılamaz.");
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        uow.Users.Update(user);
        await AddAuditAsync(actingAdminId, isActive ? "user.activated" : "user.deactivated", "User", id,
            $"{user.Email} hesabı {(isActive ? "etkinleştirildi" : "devre dışı bırakıldı")}.");
        await uow.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync()
    {
        var categories = await uow.Categories.GetAllCategoriesWithDetailsAsync();
        return categories.Select(c => new AdminCategoryDto(
            c.Id,
            c.Name,
            c.ParentId,
            c.Parent?.Name,
            c.IsActive,
            c.Adverts.Count(a => a.IsActive)))
        .ToList();
    }

    public async Task<ServiceResult<int>> CreateCategoryAsync(AdminCategoryUpsertDto dto, int adminUserId)
    {
        var validation = await ValidateCategoryAsync(dto, null);
        if (validation is not null) return ServiceResult<int>.Failure(validation.Value.Error, validation.Value.Message);
        var category = new Category { Name = dto.Name.Trim(), ParentId = dto.ParentId, IsActive = true, CreatedAt = DateTime.UtcNow };
        await uow.BeginTransactionAsync();
        try
        {
            await uow.Categories.AddAsync(category);
            await uow.SaveChangesAsync();
            await AddAuditAsync(adminUserId, "category.created", "Category", category.Id, $"'{category.Name}' kategorisi oluşturuldu.");
            await uow.SaveChangesAsync();
            await uow.CommitAsync();
            return ServiceResult<int>.Success(category.Id);
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }

    public async Task<ServiceResult<bool>> UpdateCategoryAsync(int id, AdminCategoryUpsertDto dto, int adminUserId)
    {
        var category = await uow.Categories.GetByIdAsync(id);
        if (category is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kategori bulunamadı.");
        var validation = await ValidateCategoryAsync(dto, id);
        if (validation is not null) return ServiceResult<bool>.Failure(validation.Value.Error, validation.Value.Message);
        category.Name = dto.Name.Trim();
        category.ParentId = dto.ParentId;
        category.IsActive = true;
        category.UpdatedAt = DateTime.UtcNow;
        uow.Categories.Update(category);
        await AddAuditAsync(adminUserId, "category.updated", "Category", id, $"Kategori adı '{category.Name}' olarak güncellendi.");
        await uow.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id, int adminUserId)
    {
        var category = await uow.Categories.GetByIdAsync(id);
        if (category is null || !category.IsActive) return ServiceResult<bool>.Failure(ServiceError.NotFound, "Kategori bulunamadı.");
        if (await uow.Categories.AnyAsync(c => c.ParentId == id && c.IsActive))
            return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "Önce bu kategoriye bağlı alt kategorileri taşıyın veya silin.");
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        uow.Categories.Update(category);
        await AddAuditAsync(adminUserId, "category.deactivated", "Category", id, $"'{category.Name}' kategorisi silindi.");
        await uow.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdminAuditLogDto>> GetAuditLogsAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 200);
        var logs = await uow.AuditLogs.GetRecentLogsWithUserAsync(limit);
        return logs.Select(log => new AdminAuditLogDto(
            log.Id,
            log.AdminUserId,
            log.AdminUser is not null ? $"{log.AdminUser.FirstName} {log.AdminUser.LastName}" : string.Empty,
            log.Action,
            log.TargetType,
            log.TargetId,
            log.Details,
            log.CreatedAt))
        .ToList();
    }

    private async Task AddAuditAsync(int adminUserId, string action, string targetType, int targetId, string details) =>
        await uow.AuditLogs.AddAsync(new AdminAuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

    private async Task<(ServiceError Error, string Message)?> ValidateCategoryAsync(AdminCategoryUpsertDto dto, int? id)
    {
        var name = dto.Name.Trim();
        if (await uow.Categories.AnyAsync(c => c.Id != id && c.IsActive && c.Name.ToLower() == name.ToLower()))
            return (ServiceError.Conflict, "Bu isimde aktif bir kategori zaten var.");
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId == id) return (ServiceError.InvalidOperation, "Kategori kendisinin üst kategorisi olamaz.");
            if (!await uow.Categories.AnyAsync(c => c.Id == dto.ParentId && c.IsActive && c.ParentId == null))
                return (ServiceError.InvalidOperation, "Üst kategori olarak yalnızca ana kategoriler seçilebilir.");
            if (id.HasValue && await uow.Categories.AnyAsync(c => c.ParentId == id && c.IsActive))
                return (ServiceError.InvalidOperation, "Alt kategorisi bulunan bir ana kategori alt kategoriye dönüştürülemez.");
        }
        return null;
    }
}
