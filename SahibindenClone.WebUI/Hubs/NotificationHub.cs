using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SahibindenClone.WebUI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {

    }
}