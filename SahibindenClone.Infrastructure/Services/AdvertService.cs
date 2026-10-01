using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Interfaces;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Entities;
using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Infrastructure.Services;

public sealed class AdvertService(IAdvertRepository adverts, INotificationService notifications) : IAdvertService
{
    public async Task<PaginatedResultDto<AdvertListDto>> GetAdvertsAsync(string? search, int? categoryId, int? cityId,
        decimal? minPrice, decimal? maxPrice, string? brand, string? model, DateOnly? createdFrom, DateOnly? createdTo,
        string? sortBy, int page, int pageSize)
    {
        page = Math.Max(1, page);
        if (pageSize < 1) pageSize = 10;
        pageSize = Math.Min(pageSize, 100);
        var (items, count) = await adverts.GetFilteredAdvertsAsync(search, categoryId, cityId, minPrice, maxPrice,
            brand, model, createdFrom, createdTo, sortBy, page, pageSize);
        return new PaginatedResultDto<AdvertListDto>
        {
            Items = items.Select(a => new AdvertListDto
            {
                Id = a.Id, Title = a.Title, Price = a.Price, Brand = a.Brand, Model = a.Model, CategoryName = a.Category?.Name ?? "Kategorisiz",
                CityName = a.City?.Name ?? "Belirtilmemiş", UserName = $"{a.User?.FirstName} {a.User?.LastName}",
                Status = (int)a.Status, StatusName = StatusName(a.Status), CreatedAt = a.CreatedAt,
                ImageUrl = MainImage(a.Images)
            }).ToList(),
            TotalCount = count, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdvertDetailDto?> GetByIdAsync(int id)
    {
        var a = await adverts.GetAdvertWithDetailsByIdAsync(id);
        return a is null || a.Status is AdvertStatus.PendingApproval or AdvertStatus.Rejected ? null : new AdvertDetailDto(a.Id, a.Title, a.Price, a.Description, a.Brand, a.Model, a.CategoryId,
            a.Category?.Name ?? "Kategorisiz", a.CityId, a.City?.Name ?? "Belirtilmemiş",
            $"{a.User?.FirstName} {a.User?.LastName}", a.UserId, (int)a.Status, StatusName(a.Status), a.CreatedAt,
            a.Images.OrderBy(i => i.SortOrder).Select(i => new AdvertImageDto(i.Id, i.ImageUrl, i.IsMain, i.SortOrder)).ToList());
    }

    public async Task<ServiceResult<bool>> CanUpdateAsync(int id, int userId)
    {
        var advert = await adverts.GetAdvertWithDetailsByIdAsync(id);
        if (advert is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.UserId != userId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu ilanı düzenleme yetkiniz yok.");
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<int>> CreateAsync(AdvertCreateDto dto, int userId, IReadOnlyCollection<AdvertImage>? images)
    {
        var advert = new Advert
        {
            Title = dto.Title, Description = dto.Description, Price = dto.Price, CategoryId = dto.CategoryId,
            Brand = dto.Brand?.Trim(), Model = dto.Model?.Trim(),
            CityId = dto.CityId, UserId = userId, Status = AdvertStatus.PendingApproval,
            ExpirationDate = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow,
            Images = images?.ToList() ?? new List<AdvertImage>()
        };
        await adverts.AddAsync(advert);
        await adverts.SaveChangesAsync();
        return ServiceResult<int>.Success(advert.Id);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, AdvertUpdateDto dto, int userId, IReadOnlyCollection<AdvertImage>? images)
    {
        var advert = await adverts.GetAdvertWithDetailsByIdAsync(id);
        if (advert is null) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.UserId != userId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu ilanı düzenleme yetkiniz yok.");
        
        bool isPriceDropped = dto.Price < advert.Price;
        if (advert.Title != dto.Title || advert.Description != dto.Description || advert.Price != dto.Price)
        {
            await adverts.AddChangeHistoryAsync(new AdvertChangeHistory
            {
                AdvertId = advert.Id, ChangedByUserId = userId,
                PreviousTitle = advert.Title, NewTitle = dto.Title,
                PreviousDescription = advert.Description, NewDescription = dto.Description,
                PreviousPrice = advert.Price, NewPrice = dto.Price, CreatedAt = DateTime.UtcNow
            });
        }
        
        advert.Title = dto.Title; 
        advert.Description = dto.Description; 
        advert.Price = dto.Price;
        advert.Brand = dto.Brand?.Trim();
        advert.Model = dto.Model?.Trim();
        advert.CategoryId = dto.CategoryId; 
        advert.CityId = dto.CityId; 
        advert.UpdatedAt = DateTime.UtcNow;

        if (images is { Count: > 0 })
        {
            var sortOrder = advert.Images.Count == 0 ? 0 : advert.Images.Max(i => i.SortOrder) + 1;
            foreach (var image in images)
            {
                image.SortOrder = sortOrder++;
                image.IsMain = advert.Images.Count == 0 && sortOrder == 1;
                advert.Images.Add(image);
            }
        }

        adverts.Update(advert);
        await adverts.SaveChangesAsync();

        if (isPriceDropped)
        {
            await notifications.NotifyPriceDropAsync(advert.Id, advert.Title, dto.Price);
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> MarkAsSoldAsync(int id, int userId)
    {
        var advert = await adverts.GetByIdAsync(id);
        if (advert is null || !advert.IsActive) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.UserId != userId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu işlemi yapma yetkiniz yok.");
        if (advert.Status == AdvertStatus.Sold) return ServiceResult<bool>.Failure(ServiceError.InvalidOperation, "İlan zaten satıldı olarak işaretlenmiş.");
        advert.Status = AdvertStatus.Sold; advert.UpdatedAt = DateTime.UtcNow;
        adverts.Update(advert); await adverts.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int userId)
    {
        var advert = await adverts.GetByIdAsync(id);
        if (advert is null || !advert.IsActive) return ServiceResult<bool>.Failure(ServiceError.NotFound, "İlan bulunamadı.");
        if (advert.UserId != userId) return ServiceResult<bool>.Failure(ServiceError.Forbidden, "Bu ilanı silme yetkiniz yok.");
        advert.IsActive = false; advert.UpdatedAt = DateTime.UtcNow;
        adverts.Update(advert); await adverts.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<AdvertChangeHistoryDto>> GetChangeHistoryAsync(int advertId)
    {
        var history = await adverts.GetChangeHistoryAsync(advertId);
        return history.Select(h => new AdvertChangeHistoryDto(h.Id, h.AdvertId, h.ChangedByUserId,
            $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}", h.PreviousTitle, h.NewTitle,
            h.PreviousDescription, h.NewDescription, h.PreviousPrice, h.NewPrice, h.CreatedAt)).ToList();
    }

    private static string? MainImage(IEnumerable<AdvertImage>? images) => images?.OrderBy(i => i.SortOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl
        ?? images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl;

    private static string StatusName(AdvertStatus status) => status switch
    {
        AdvertStatus.Active => "Aktif", AdvertStatus.Sold => "Satıldı", AdvertStatus.Expired => "Süresi Doldu",
        AdvertStatus.Rejected => "Reddedildi", _ => "Onay Bekliyor"
    };
}
