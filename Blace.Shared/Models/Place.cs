namespace Blace.Shared.Models;

public record Place(
        string Id,
        string Title,
        DateTime CreatedTimeUtc,
        DateTime LastChangeTimeUtc,
        byte[]? Canvas,
        int Height = 128,
        int Width = 128)
    : PlaceInfo(Id, Title, CreatedTimeUtc, LastChangeTimeUtc)
{
    public int Height { get; set; } = Height;
    public int Width { get; set; } = Width;
}