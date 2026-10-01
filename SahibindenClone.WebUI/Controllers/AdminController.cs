using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Services;
using SahibindenClone.Domain.Enums;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService admin) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => Ok(await admin.GetDashboardAsync());

    [HttpGet("adverts")]
    public async Task<IActionResult> Adverts([FromQuery] AdvertStatus? status) => Ok(await admin.GetAdvertsAsync(status));

    [HttpPut("adverts/{id:int}/decision")]
    public async Task<IActionResult> DecideAdvert(int id, [FromBody] AdvertDecisionDto dto)
    {
        if (!TryGetAdminId(out var adminId)) return Unauthorized();
        var status = dto.Decision.Trim().ToLowerInvariant() switch
        {
            "approve" => AdvertStatus.Active,
            "reject" => AdvertStatus.Rejected,
            _ => (AdvertStatus?)null
        };
        if (!status.HasValue) return BadRequest(new { message = "Karar approve veya reject olmalıdır." });
        var result = await admin.SetAdvertStatusAsync(id, status.Value, adminId);
        return Result(result, "İlan kararı kaydedildi.");
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users() => Ok(await admin.GetUsersAsync());

    [HttpPut("users/{id:int}/active")]
    public async Task<IActionResult> SetUserActive(int id, [FromBody] SetUserActiveDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId)) return Unauthorized();
        return Result(await admin.SetUserActiveAsync(id, dto.IsActive, adminId), "Kullanıcı durumu güncellendi.");
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() => Ok(await admin.GetCategoriesAsync());

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] AdminCategoryUpsertDto dto)
    {
        if (!TryGetAdminId(out var adminId)) return Unauthorized();
        var result = await admin.CreateCategoryAsync(dto, adminId);
        return result.Succeeded ? Created($"/api/admin/categories/{result.Value}", new { id = result.Value }) : Failure(result);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminCategoryUpsertDto dto)
    {
        if (!TryGetAdminId(out var adminId)) return Unauthorized();
        return Result(await admin.UpdateCategoryAsync(id, dto, adminId), "Kategori güncellendi.");
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!TryGetAdminId(out var adminId)) return Unauthorized();
        return Result(await admin.DeleteCategoryAsync(id, adminId), "Kategori silindi.");
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] int limit = 100) => Ok(await admin.GetAuditLogsAsync(limit));

    private bool TryGetAdminId(out int adminId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out adminId);

    private IActionResult Result<T>(ServiceResult<T> result, string message) => result.Succeeded ? Ok(new { message }) : Failure(result);
    private IActionResult Failure<T>(ServiceResult<T> result) => result.Error switch
    {
        ServiceError.NotFound => NotFound(new { message = result.Message }),
        ServiceError.Forbidden => StatusCode(403, new { message = result.Message }),
        ServiceError.Conflict => Conflict(new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}

public sealed class AdvertDecisionDto { public string Decision { get; set; } = string.Empty; }
public sealed class SetUserActiveDto { public bool IsActive { get; set; } }
