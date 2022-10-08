using Blace.Shared;
using Blace.Shared.Models;

namespace Blace.Client.Services;

public class PlayerService : HubClient
{
    private readonly IServer _server;

    public PlayerService(IServer server)
    {
        _server = server;
    }

    public event Action? Changed;
    public Player Player { get; private set; } = null!;
    public List<Player> Players { get; private set; } = new();
    public List<Player> PlayersWithName { get; private set; } = new();

    public async Task Initialize()
    {
        Player = await _server.GetMe();
    }
    
    public override Task UpdatePlayers(List<Player> players)
    {
        Players = players;
        PlayersWithName = players.Where(p => p.Name != null).ToList();
        Changed?.Invoke();
        return Task.CompletedTask;
    }
}