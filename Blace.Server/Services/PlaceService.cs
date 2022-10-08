using System.Collections.Concurrent;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class PlaceService
{
    private readonly IHubContext<Server, IClient> _hub;
    private readonly PlaceRepository _placeRepository;

    private readonly Throttler _canvasUpdater;
    private readonly Throttler _pixelsUpdater;
    private readonly ConcurrentQueue<(byte, byte)> _changedPixels = new();
    private readonly ConcurrentQueue<Tile> _tiles = new();
    private readonly ConcurrentDictionary<Guid, byte> _bannedPlayers = new();

    public uint Cooldown { get; private set; }
    public Place Place { get; private set; } = null!;

    public PlaceService(IHubContext<Server, IClient> hub, PlaceRepository placeRepository)
    {
        _hub = hub;
        _placeRepository = placeRepository;
        _canvasUpdater = new(60000, () =>
        {
            hub.Clients.All.UpdatePlace(Place);
            _ = Save();
        });
        _pixelsUpdater = new(1000, () =>
        {
            List<Pixel> pixels = new(_changedPixels.Count);
            while (_changedPixels.TryDequeue(out (byte X, byte Y) coordinate))
            {
                (byte x, byte y) = coordinate;
                Pixel pixel = new(x, y, CanvasGetColor(x, y));
                pixels.Add(pixel);
            }

            hub.Clients.All.UpdatePixels(pixels.ToArray());
        });
    }

    public byte[] Canvas => Place.Canvas!;

    public async Task Initialize()
    {
        if (_placeRepository.Places.Count > 0)
            await SetPlace(_placeRepository.Places.OrderByDescending(p => p.LastChangeTimeUtc).First().Id);
        else
            await CreateNewPlace();
    }

    public async Task SetPlace(string id)
    {
        if (Place?.Id == id) return;
        await SetPlace(await _placeRepository.Get(id));
    }

    public void SetPixel(int x, int y, byte color, Guid userId)
    {
        if (_bannedPlayers.ContainsKey(userId)) return;
        if (color >> 4 != 0) throw new InvalidOperationException();

        CanvasSetColor(x, y, color, out byte previousColor);
        _changedPixels.Enqueue(((byte)x, (byte)y));
        _tiles.Enqueue(new(Guid.NewGuid().ToString(), DateTime.UtcNow, Place.Id, userId, (ushort)x, (ushort)y, color,
            previousColor));
        _canvasUpdater.Update();
        _pixelsUpdater.Update();
    }

    private async Task Save()
    {
        await _placeRepository.Save(Place);
        List<Tile> tiles = new(_tiles.Count);
        while (_tiles.TryDequeue(out Tile? tile))
        {
            tiles.Add(tile);
        }

        await _placeRepository.SaveTiles(tiles);
    }

    public async Task CreateNewPlace()
    {
        await SetPlace(
            new Place(
                Guid.NewGuid().ToString(),
                "",
                DateTime.UtcNow,
                DateTime.UtcNow,
                new byte[64 * 128]));
        _placeRepository.Places.Insert(0, Place);
    }

    private async Task SetPlace(Place place)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Place != null)
            _ = Save();
        Place = place;
        _ = Save();
        await _hub.Clients.All.UpdatePlace(Place);
    }

    public async Task SetCooldown(uint cooldown)
    {
        Cooldown = cooldown;
        await _hub.Clients.All.UpdateCooldown(cooldown);
    }

    public async Task Delete(PlaceInfo place)
    {
        await _placeRepository.Delete(place);
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte lastColor)
    {
        await Save();
        return await _placeRepository.GetTilesBySamePlayer(x, y, lastColor, Place.Id);
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        Guid userId = tiles[0].UserId;
        _bannedPlayers[userId] = 0;
        await _placeRepository.DeleteTiles(tiles);
        foreach (Tile tile in tiles)
        {
            if (CanvasGetColor(tile.X, tile.Y) != tile.Color) continue;
            CanvasSetColor(tile.X, tile.Y, tile.PreviousColor, out _);
            _changedPixels.Enqueue(((byte)tile.X, (byte)tile.Y));
        }

        _canvasUpdater.Update();
        _pixelsUpdater.Update();
    }

    private byte CanvasGetColor(int x, int y) => Canvas[x / 2 + y * 64].GetNibble(x);

    private void CanvasSetColor(int x, int y, byte color, out byte previousColor)
    {
        byte b = Canvas[x / 2 + y * 64];
        previousColor = b.GetNibble(x);
        Canvas[x / 2 + y * 64] = b.WithNibble(x, color);
    }
}