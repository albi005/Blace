using Blace.Shared.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Blace.Console;

public static class Migrate
{
    public static async Task Execute()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string connectionString = configuration["CosmosDb:ConnectionString"];

        CosmosClient client = new(connectionString, new() { AllowBulkExecution = true });

        Container tilesContainer = client.GetContainer("place", "tiles");

        FeedIterator<OldTile> feedIterator = tilesContainer.GetItemLinqQueryable<OldTile>()
            .Where(tile => tile.IsDeleted && tile.PlaceId == "fa30bf1a-83c7-4bd2-aa97-b450404f8140")
            .ToFeedIterator();

        FeedResponse<OldTile> response = await feedIterator.ReadNextAsync();

        PartitionKey partitionKey = new("fa30bf1a-83c7-4bd2-aa97-b450404f8140");
        await Task.WhenAll(response.Select(t => tilesContainer.PatchItemAsync<OldTile>(t.Id, partitionKey, new[]
        {
            PatchOperation.Set('/' + nameof(Tile.DeleteId), Guid.Empty),
        })));
    }
}

public class OldTile
{
    [JsonProperty("id")]
    public string Id { get; set; }
    public bool IsDeleted { get; set; }
    public string PlaceId { get; set; }
}