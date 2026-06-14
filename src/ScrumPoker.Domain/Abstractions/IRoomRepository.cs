using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Domain.Abstractions;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Room> SaveAsync(Room room, TimeSpan? expiry = null, CancellationToken ct = default);
}
