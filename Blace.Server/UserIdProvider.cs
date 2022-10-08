using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.Features.Get<IHttpContextFeature>()!.HttpContext!.Request.Query["access_token"];
    }
}