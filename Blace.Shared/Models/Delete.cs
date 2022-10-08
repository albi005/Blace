using Newtonsoft.Json;

namespace Blace.Shared.Models;

public record Delete(string Id, DateTime DateTimeUtc, Guid UserId)
{
    [JsonProperty("id")]
    public string Id { get; } = Id;
}