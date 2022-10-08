namespace Blace.Shared.Models;

public record Player(Guid Id)
{
    public string? Name { get; set; } = null!;
    public DateTimeOffset JoinTime { get; } = DateTimeOffset.UtcNow;
    public int Score { get; set; }
    public bool IsHidden { get; set; }
    public bool IsConnected { get; set; } = true;
}
