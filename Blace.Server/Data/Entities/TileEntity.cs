namespace Blace.Server.Data.Entities;

public class TileEntity
{
    public string Id { get; set; } = null!;
    public DateTime CreatedTimeUtc { get; set; }
    public string PlaceId { get; set; } = null!;
    public Guid UserId { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public byte Color { get; set; }
    public byte PreviousColor { get; set; }
    public string? DeleteId { get; set; }
}
