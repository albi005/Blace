using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class StateService
{
    private readonly IHubContext<Server, IClient> _hub;

    public StateService(IHubContext<Server, IClient> hub)
    {
        _hub = hub;
    }

    public State State { get; private set; }

    public async Task SetState(State state)
    {
        State = state;
        await _hub.Clients.All.UpdateState(state);
    }
}