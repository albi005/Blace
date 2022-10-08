using Newtonsoft.Json;

namespace Blace.Shared.Models;

public record Tile(
    string Id,
    DateTime CreatedTimeUtc,
    string PlaceId,
    Guid UserId,
    ushort X,
    ushort Y,
    byte Color,
    byte PreviousColor)
{
    [JsonProperty("id")]
    public string Id { get; } = Id;
    
    public string? DeleteId { get; set; }
}