namespace ScrumPoker.Domain.Rooms;

public sealed record Room
{
    public int Id { get; init; }
    public List<Player> Players { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
