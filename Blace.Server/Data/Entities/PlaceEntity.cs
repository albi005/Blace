namespace Blace.Server.Data.Entities;

public class PlaceEntity
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime CreatedTimeUtc { get; set; }
    public DateTime LastChangeTimeUtc { get; set; }
    public byte[]? Canvas { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}
