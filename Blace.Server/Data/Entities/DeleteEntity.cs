namespace Blace.Server.Data.Entities;

public class DeleteEntity
{
    public string Id { get; set; } = null!;
    public DateTime DateTimeUtc { get; set; }
    public Guid UserId { get; set; }
}
