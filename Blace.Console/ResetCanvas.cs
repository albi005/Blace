using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;

namespace Blace.Console;

public static class ResetCanvas
{
    public static async Task Execute()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string connectionString = configuration["CosmosDb:ConnectionString"];

        CosmosClient client = new(connectionString, new() { AllowBulkExecution = true });

        Container placesContainer = client.GetContainer("place", "places");
        
        Place place = (await placesContainer
            .GetItemLinqQueryable<Place>()
            .Where(p => p.Id == "fa30bf1a-83c7-4bd2-aa97-b450404f8140")
            .Take(1)
            .ToFeedIterator()
            .ReadNextAsync())
            .First();
        
        Container tilesContainer = client.GetContainer("place", "tiles");

        List<Tile> tiles = new();

        FeedIterator<Tile> feedIterator = tilesContainer.GetItemLinqQueryable<Tile>()
            .Where(t => t.PlaceId == "fa30bf1a-83c7-4bd2-aa97-b450404f8140" &&
                        (!t.DeleteId.IsDefined() || t.DeleteId == null))
            .ToFeedIterator();

        while (feedIterator.HasMoreResults)
            tiles.AddRange(await feedIterator.ReadNextAsync());

        byte[] canvas = place.Canvas!;
        foreach (Tile tile in tiles)
        {
            byte b = canvas[tile.X / 2 + tile.Y * 64];
            canvas[tile.X / 2 + tile.Y * 64] = b.WithNibble(tile.X, tile.Color > 15
                ? tile.Color.GetNibble(0)
                : tile.Color);
        }

        await placesContainer.UpsertItemAsync(place, new(place.Id));
    }
}