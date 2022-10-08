using Blace.Shared.Models;

namespace Blace.Shared;

public interface IServer
{
    Task<State> GetState();
    Task<Player> GetMe();
    Task SetName(string name);
    Task AnswerQuestion(bool isCorrect);
    Task<Place> GetPlace();
    Task<uint> GetCooldown();
    Task PlaceTile(int x, int y, byte color);
    Task Vote(byte index);
    Task<List<Tile>?> GetTilesBySamePlayer(int x, int y, byte color);
    Task DeleteTiles(Tile[] tiles);
}