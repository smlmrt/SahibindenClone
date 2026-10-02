using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SahibindenClone.WebUI.Hubs;

/// <summary>
/// SignalR bağlantılarını JWT token'daki NameIdentifier claim'i ile eşleştirir.
/// </summary>
public sealed class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
