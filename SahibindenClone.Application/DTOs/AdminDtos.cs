using System.ComponentModel.DataAnnotations;

namespace SahibindenClone.Application.DTOs;

public sealed record AdminAdvertDto(int Id, string Title, string Description, decimal Price, string OwnerName, string CategoryName,
    string CityName, int Status, string StatusName, DateTime CreatedAt, string? ImageUrl);

public sealed record AdminUserDto(int Id, string FirstName, string LastName, string Email, string Role,
    bool IsActive, DateTime CreatedAt, int AdvertCount);

public sealed record AdminDashboardDto(int UserCount, int ActiveUserCount, int AdvertCount,
    int PendingAdvertCount, int ActiveAdvertCount, int RejectedAdvertCount, int CategoryCount);

public sealed class AdminCategoryUpsertDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int? ParentId { get; set; }
}
