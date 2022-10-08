using Microsoft.AspNetCore.SignalR;

namespace Blace.Server;

public static class Extensions
{
    public static Guid GetId(this HubCallerContext context)
        => Guid.ParseExact(context.UserIdentifier ?? throw new("UserId is null"), "D");
}