using Blace.Shared.Models;

namespace Blace.Server.Services;

public class InMemoryPlaceRepository : IPlaceRepository
{
    private readonly LinkedList<Tile> _tiles = new();

    public List<PlaceInfo> Places { get; } = new()
    {
        new Place("the chosen one", "Place 0", DateTime.UtcNow, DateTime.UtcNow, new byte[128 * 128])
    };
    
    public Task<Place> Get(string placeId) => Task.FromResult((Place)Places.First(p => p.Id == placeId));

    public Task Save(Place place) => Task.CompletedTask;

    public Task SaveTiles(IEnumerable<Tile> tiles)
    {
        foreach (Tile tile in tiles) _tiles.AddLast(tile);
        return Task.CompletedTask;
    }
    
    public Task Delete(PlaceInfo place)
    {
        Places.Remove(place);
        return Task.CompletedTask;
    }

    public Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId)
    {
        Guid? userId = _tiles
            .FirstOrDefault(t => t.X == x && t.Y == y && t.Color == color && t.PlaceId == placeId)?
            .UserId;
        return Task.FromResult(_tiles.Where(t => t.UserId == userId && t.PlaceId == placeId).ToList());
    }

    public Task DeleteTiles(Tile[] tiles)
    {
        foreach (Tile tile in tiles) _tiles.Remove(tile);
        return Task.CompletedTask;
    }
}