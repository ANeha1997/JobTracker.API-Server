using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JobTracker.API.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
    }
}
