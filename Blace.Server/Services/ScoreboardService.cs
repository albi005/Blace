using Blace.Shared.Models;

namespace Blace.Server.Services;

public class ScoreboardService
{
    private readonly PlayerService _playerService;

    public ScoreboardService(PlayerService playerService)
    {
        _playerService = playerService;
    }
    
    public void Reset()
    {
        foreach (Player player in _playerService.All)
            player.Score = 0;
        _playerService.Update();
    }
}