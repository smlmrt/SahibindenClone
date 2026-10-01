using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Services;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public sealed class PurchasesController(IPurchaseService purchases) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return Ok(await purchases.GetMyPurchasesAsync(userId));
    }

    [HttpPost]
    public async Task<IActionResult> RequestPurchase([FromBody] PurchaseRequestDto dto)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var result = await purchases.RequestPurchaseAsync(dto.AdvertId, userId);
        return result.Succeeded ? Created($"/api/purchases/{result.Value}", new { id = result.Value }) : Fail(result);
    }

    [HttpPut("{id:int}/decision")]
    public async Task<IActionResult> Decide(int id, [FromBody] PurchaseDecisionDto dto)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return Result(await purchases.DecideAsync(id, userId, dto.Accept), "Talep yanıtlandı.");
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return Result(await purchases.ConfirmCompletionAsync(id, userId), "Alışveriş tamamlandı olarak onaylandı.");
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] SellerReviewCreateDto dto)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return Result(await purchases.AddReviewAsync(id, userId, dto), "Satıcı değerlendirmesi kaydedildi.");
    }

    private bool TryUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    private IActionResult Result<T>(ServiceResult<T> result, string message) => result.Succeeded ? Ok(new { message }) : Fail(result);
    private IActionResult Fail<T>(ServiceResult<T> result) => result.Error switch
    {
        ServiceError.NotFound => NotFound(new { message = result.Message }),
        ServiceError.Forbidden => StatusCode(403, new { message = result.Message }),
        ServiceError.Conflict => Conflict(new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}
