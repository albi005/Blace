using Newtonsoft.Json;

namespace Blace.Shared.Models;

public record PlaceInfo(
    string Id,
    string Title,
    DateTime CreatedTimeUtc,
    DateTime LastChangeTimeUtc)
{
    [JsonProperty("id")]
    public string Id { get; } = Id;

    public string Title { get; set; } = Title;
    public DateTime LastChangeTimeUtc { get; set; } = LastChangeTimeUtc;
}