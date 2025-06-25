using Blace.Server.Services;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sentry;

namespace Blace.Server;

public class Server : Hub<IClient>, IServer
{
    private readonly PlayerService _playerService;
    private readonly PlaceService _placeService;
    private readonly StateService _stateService;
    private readonly QuestionService _questionService;
    private readonly VoteService _voteService;

    public Server(
        PlayerService playerService,
        PlaceService placeService,
        StateService stateService,
        QuestionService questionService, VoteService voteService)
    {
        _playerService = playerService;
        _placeService = placeService;
        _stateService = stateService;
        _questionService = questionService;
        _voteService = voteService;
    }

    public override Task OnConnectedAsync()
    {
        _playerService[Context].IsConnected = true;
        _playerService.Update();
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            SentrySdk.CaptureException(exception);
        _playerService[Context].IsConnected = false;
        _playerService.Update();
        return Task.CompletedTask;
    }

    public Task<State> GetState() => Task.FromResult(_stateService.State);
    public Task<Player> GetMe() => Task.FromResult(_playerService[Context]);
    public Task<Place> GetPlace() => Task.FromResult(_placeService.Place);
    public Task<uint> GetCooldown() => Task.FromResult(_placeService.Cooldown);

    public Task PlaceTile(int x, int y, byte color)
    {
        _placeService.SetPixel(x, y, color, Context.GetId());
        return Task.CompletedTask;
    }

    public Task Vote(byte index)
    {
        _voteService.Vote(Context.GetId(), index);
        return Task.CompletedTask;
    }

    public async Task<List<Tile>?> GetTilesBySamePlayer(int x, int y, byte color)
    {
        try
        {
            return await _placeService.GetTilesBySamePlayer(x, y, color);
        }
        catch (TileNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        if (_playerService[Context].Id != _placeService.AdminUserId)
            return;
        await _placeService.DeleteTiles(tiles);
    }

    public Task SetName(string name)
    {
        _playerService[Context].Name = name;
        _playerService.Update();
        return Task.CompletedTask;
    }

    public Task AnswerQuestion(bool isCorrect)
    {
        _questionService.AnswerQuestion(_playerService[Context], isCorrect);
        return Task.CompletedTask;
    }
}