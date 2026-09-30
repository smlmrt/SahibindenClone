using SahibindenClone.Application.DTOs;
using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Application.Services;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<IReadOnlyList<AdminAdvertDto>> GetAdvertsAsync(AdvertStatus? status);
    Task<ServiceResult<bool>> SetAdvertStatusAsync(int id, AdvertStatus status);
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync();
    Task<ServiceResult<bool>> SetUserActiveAsync(int id, bool isActive, int actingAdminId);
    Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync();
    Task<ServiceResult<int>> CreateCategoryAsync(AdminCategoryUpsertDto dto);
    Task<ServiceResult<bool>> UpdateCategoryAsync(int id, AdminCategoryUpsertDto dto);
    Task<ServiceResult<bool>> DeleteCategoryAsync(int id);
}

public sealed record AdminCategoryDto(int Id, string Name, int? ParentId, string? ParentName, bool IsActive, int AdvertCount);
