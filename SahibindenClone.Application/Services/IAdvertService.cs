using SahibindenClone.Application.DTOs;
using SahibindenClone.Domain.Entities;

namespace SahibindenClone.Application.Services;

public sealed record AdvertDetailDto(
    int Id, string Title, decimal Price, string Description, string? Brand, string? Model, int CategoryId, string CategoryName,
    int CityId, string CityName, string UserName, int UserId, int Status, string StatusName,
    DateTime CreatedAt, IReadOnlyList<AdvertImageDto> Images);

public sealed record AdvertImageDto(int Id, string ImageUrl, bool IsMain, int SortOrder);

public interface IAdvertService
{
    Task<PaginatedResultDto<AdvertListDto>> GetAdvertsAsync(string? search, int? categoryId, int? cityId,
        decimal? minPrice, decimal? maxPrice, string? brand, string? model, DateOnly? createdFrom, DateOnly? createdTo,
        string? sortBy, int page, int pageSize);
    Task<AdvertDetailDto?> GetByIdAsync(int id);
    Task<ServiceResult<bool>> CanUpdateAsync(int id, int userId);
    Task<ServiceResult<int>> CreateAsync(AdvertCreateDto dto, int userId, IReadOnlyCollection<AdvertImage>? images);
    Task<ServiceResult<bool>> UpdateAsync(int id, AdvertUpdateDto dto, int userId, IReadOnlyCollection<AdvertImage>? images);
    Task<ServiceResult<bool>> MarkAsSoldAsync(int id, int userId);
    Task<ServiceResult<bool>> DeleteAsync(int id, int userId);
    Task<IReadOnlyList<AdvertChangeHistoryDto>> GetChangeHistoryAsync(int advertId);
}
