using Blace.Shared.Models;

namespace Blace.Server.Services;

public interface IPlaceRepository
{
    List<PlaceInfo> Places { get; }
    Task Initialize();
    Task<Place> Get(string placeId);
    Task Save(Place place);
    Task SaveTiles(IEnumerable<Tile> tiles);
    Task Delete(PlaceInfo place);
    Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId);
    Task DeleteTiles(Tile[] tiles);
}