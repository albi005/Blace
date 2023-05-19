using System.Diagnostics;
using System.Net.Sockets;
using Blace.Shared.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace Blace.Server.Services;

public class CosmosDbPlaceRepository : IPlaceRepository
{
    private readonly CosmosClient _cosmosClient;
    private Database _database = null!;
    private Container _tiles = null!;
    private Container _places = null!;
    private Container _deletes = null!;

    public CosmosDbPlaceRepository(string connectionString)
    {
        _cosmosClient = new(connectionString, new() { AllowBulkExecution = true });
    }

    public List<PlaceInfo> Places { get; private set; } = null!;

    public async Task Initialize()
    {
        if (_cosmosClient.Endpoint.OriginalString == "https://localhost:8081/")
            EnsureEmulatorIsRunning();
        
        _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
            "place",
            1000);

        _tiles = await _database.CreateContainerIfNotExistsAsync(
            "tiles",
            "/" + nameof(Tile.PlaceId));

        _places = await _database.CreateContainerIfNotExistsAsync(
            "places",
            "/id");
        
        _deletes = await _database.CreateContainerIfNotExistsAsync(
            "deletes",
            "/id");

        Places = await GetPlaces();
    }

    private async Task<List<PlaceInfo>> GetPlaces()
    {
        List<PlaceInfo> result = new();
        using FeedIterator<PlaceInfo> feed = _places
            .GetItemLinqQueryable<PlaceInfo>()
            .OrderByDescending(p => p.CreatedTimeUtc)
            .ToFeedIterator();
        while (feed.HasMoreResults)
        {
            FeedResponse<PlaceInfo> response = await feed.ReadNextAsync();
            result.AddRange(response);
        }

        return result;
    }

    public async Task<Place> Get(string placeId)
    {
        Place place = await _places.ReadItemAsync<Place>(placeId, new(placeId));
        if (place.Width == 0) place.Height = place.Width = 128;
        return place;
    }

    public async Task Save(Place place)
    {
        place.Title = Places.FirstOrDefault(p => p.Id == place.Id)?.Title ?? string.Empty;
        place.LastChangeTimeUtc = DateTime.UtcNow;
        await _places.UpsertItemAsync(place);
    }

    public async Task SaveTiles(IEnumerable<Tile> tiles)
    {
        // Split the operations into chunks so CosmosDB won't complain about too many requests.
        List<Task> tasks = new();
        foreach (Tile tile in tiles)
        {
            tasks.Add(_tiles.UpsertItemAsync(tile, new(tile.PlaceId)));
            
            if (tasks.Count < 69) continue;
            await Task.WhenAll(tasks);
            tasks.Clear();
        }
    }

    public async Task Delete(PlaceInfo place)
    {
        Places.Remove(place);
        await _places.DeleteItemAsync<PlaceInfo>(place.Id, new(place.Id));
    }

    public async Task<List<Tile>> GetTilesBySamePlayer(int x, int y, byte color, string placeId)
    {
        using FeedIterator<Tile> lastTileFeed = _tiles
            .GetItemLinqQueryable<Tile>()
            .Where(t => t.PlaceId == placeId && t.Color == color && t.X == x && t.Y == y && (!t.DeleteId.IsDefined() || t.DeleteId == null))
            .OrderByDescending(t => t.CreatedTimeUtc)
            .Take(1)
            .ToFeedIterator();
        FeedResponse<Tile> response = await lastTileFeed.ReadNextAsync();
        if (response.Count == 0) throw new TileNotFoundException();

        Guid userId = response.First().UserId;

        List<Tile> tiles = new();
        using FeedIterator<Tile> tilesFeed = _tiles
            .GetItemLinqQueryable<Tile>()
            .Where(t => t.PlaceId == placeId && t.UserId == userId && (!t.DeleteId.IsDefined() || t.DeleteId == null))
            .ToFeedIterator();
        while (tilesFeed.HasMoreResults)
        {
            FeedResponse<Tile> tilesResponse = await tilesFeed.ReadNextAsync();
            tiles.AddRange(tilesResponse);
        }
        
        tiles.Sort((a,b) => b.CreatedTimeUtc.CompareTo(a.CreatedTimeUtc));

        return tiles;
    }

    public async Task DeleteTiles(Tile[] tiles)
    {
        if (tiles.Length == 0) return;

        Guid userId = tiles[0].UserId;
        string placeId = tiles[0].PlaceId;
        
        if (tiles.Any(t => t.UserId != userId || t.PlaceId != placeId))
            throw new("All of the tiles must have the same UserId and PlaceId.");
        
        Delete delete = new(Guid.NewGuid().ToString(), DateTime.UtcNow, userId);
        
        TransactionalBatch batch = _tiles.CreateTransactionalBatch(new(placeId));
        int count = 0;
        foreach (Tile tile in tiles)
        {
            count++;
            batch.PatchItem(tile.Id, new[]
            {
                PatchOperation.Add("/" + nameof(tile.DeleteId), delete.Id)
            });
            
            if (count != 100) continue;
            await ExecuteSuccessfully(batch);
            batch = _tiles.CreateTransactionalBatch(new(placeId));
            count = 0;
        }
        await ExecuteSuccessfully(batch);
        
        Array.ForEach(tiles, t => t.DeleteId = delete.Id);
        await _deletes.CreateItemAsync(delete, new(delete.Id));
    }

    private static async Task ExecuteSuccessfully(TransactionalBatch batch)
    {
        using TransactionalBatchResponse response = await batch.ExecuteAsync();
        if (!response.IsSuccessStatusCode)
            throw new CosmosException(
                response.ErrorMessage,
                response.StatusCode,
                420,
                response.ActivityId,
                response.RequestCharge);
    }

    private static void EnsureEmulatorIsRunning()
    {
        if (PingEmulator()) return;

        if (!OperatingSystem.IsWindows()) throw new CosmosDbInitException("CosmosDB emulator is not running.");

        const string emulatorPath = "C:\\Program Files\\Azure Cosmos DB Emulator\\Microsoft.Azure.Cosmos.Emulator.exe";
        if (!File.Exists(emulatorPath)) throw new CosmosDbInitException($"{emulatorPath} doesn't exist.");

        Process.Start(new ProcessStartInfo(emulatorPath, "/NoExplorer")
        {
            UseShellExecute = true,
            Verb = "runas"
        });
        Console.Write("Waiting for CosmosDB emulator to start");
        while (!PingEmulator()) { Console.Write('.'); }
        Console.WriteLine("Done");
    }

    private static bool PingEmulator()
    {
        try
        {
            new TcpClient("127.0.0.1", 8081).Dispose();
            return true;
        }
        catch (SocketException) { return false; }
    }
}

public class CosmosDbInitException : Exception
{
    public CosmosDbInitException(string message) : base(message) { }
}