using System.Collections.Concurrent;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class PlayerService
{
    private readonly IHubContext<Server, IClient> _hub;
    private readonly Throttler _throttler;
    private readonly ConcurrentDictionary<Guid, Player> _players = new();

    public PlayerService(IHubContext<Server, IClient> hub)
    {
        _hub = hub;
        _throttler = new(1000, UpdateCore);
    }

    public event Action? Changed;
    public IEnumerable<Player> All => _players.Values;
    public Player this[HubCallerContext context] => _players.GetOrAdd(context.GetId(), id => new(id));
    public List<Player> Current { get; private set; } = new();

    public void Update() => _throttler.Update();
    
    private async void UpdateCore()
    {
        Current = _players.Values
            .Where(p => p.IsConnected)
            .OrderByDescending(p => p.Score)
            .ThenByDescending(p => p.JoinTime)
            .ToList();
        Changed?.Invoke();
        await _hub.Clients.All.UpdatePlayers(Current);
    }
}