using Blace.Server.Data;
using Blace.Server.Data.Mapping;
using Blace.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Services;

public class EfPlaceRepository(IDbContextFactory<Db> dbContextFactory) : IPlaceRepository
{
    public List<PlaceInfo> Places { get; private set; } = [];

    public async Task Initialize()
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        await db.Database.MigrateAsync();

        Places = await db.Places
            .OrderByDescending(p => p.CreatedTimeUtc)
            .Select(p => new PlaceInfo(
                p.Id,
                p.Title,
                p.CreatedTimeUtc,
                p.LastChangeTimeUtc))
            .ToListAsync();
    }

    public async Task<Place> Get(string placeId)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        var placeEntity = await db.Places.FindAsync(placeId);
        if (placeEntity == null)
        {
            throw new InvalidOperationException($"Place with ID '{placeId}' not found.");
        }

        Place place = placeEntity.ToModel();

        // Handle legacy data where Width might be 0
        if (place.Width == 0)
        {
            place.Height = place.Width = 128;
        }

        return place;
    }

    public async Task Save(Place place)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        // Get existing title if it exists
        var existing = await db.Places.FindAsync(place.Id);
        if (existing != null)
        {
            place.Title = existing.Title;
        }

        place.LastChangeTimeUtc = DateTime.UtcNow;

        if (existing != null)
        {
            db.Entry(existing).CurrentValues.SetValues(place.ToEntity());
        }
        else
        {
            db.Places.Add(place.ToEntity());
            Places.Add(place);
        }

        await db.SaveChangesAsync();

        // Update in-memory list
        PlaceInfo? existingInfo = Places.FirstOrDefault(p => p.Id == place.Id);
        existingInfo?.LastChangeTimeUtc = place.LastChangeTimeUtc;
    }

    public async Task SaveTiles(IEnumerable<Tile> tiles)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        await db.Tiles.AddRangeAsync(tiles.Select(t => t.ToEntity()));
        await db.SaveChangesAsync();
    }

    public async Task Delete(PlaceInfo place)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        var existing = await db.Places.FindAsync(place.Id);
        if (existing != null)
        {
            db.Places.Remove(existing);
            await db.SaveChangesAsync();
            Places.Remove(place);
        }
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId)
    {
        await using Db db = await dbContextFactory.CreateDbContextAsync();

        // Find the last tile at the specified position with the specified color
        var lastTileEntity = await db.Tiles
            .Where(t => t.PlaceId == placeId && t.Color == color && t.X == x && t.Y == y && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .FirstOrDefaultAsync();

        if (lastTileEntity == null)
        {
            throw new TileNotFoundException();
        }

        Guid userId = lastTileEntity.UserId;

        // Get all tiles by the same user in the same place that are not deleted
        List<Tile> tiles = await db.Tiles
            .Where(t => t.PlaceId == placeId && t.UserId == userId && t.DeleteId == null)
            .OrderByDescending(t => t.CreatedTimeUtc)
            .Select(t => new Tile(
                    t.Id,
                    t.CreatedTimeUtc,
                    t.PlaceId,
                    t.UserId,
                    t.X,
                    t.Y,
                    t.Color,
                    t.PreviousColor)
                {
                    DeleteId = t.DeleteId,
                }
            )
            .ToListAsync();

        return tiles;
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        if (tiles.Length == 0) return;

        await using Db db = await dbContextFactory.CreateDbContextAsync();

        Guid userId = tiles[0].UserId;
        string placeId = tiles[0].PlaceId;

        if (tiles.Any(t => t.UserId != userId || t.PlaceId != placeId))
        {
            throw new InvalidOperationException("All of the tiles must have the same UserId and PlaceId.");
        }

        // Create a delete record
        Delete delete = new(Guid.NewGuid().ToString(), DateTime.UtcNow, userId);
        await db.Deletes.AddAsync(delete.ToEntity());

        // Update all tiles with the delete ID (soft delete)
        foreach (Tile tile in tiles)
        {
            var existingTile = await db.Tiles.FindAsync(tile.Id);
            existingTile?.DeleteId = delete.Id;
        }

        await db.SaveChangesAsync();

        // Update the passed-in tiles to reflect the change
        Array.ForEach(tiles, t => t.DeleteId = delete.Id);
    }
}