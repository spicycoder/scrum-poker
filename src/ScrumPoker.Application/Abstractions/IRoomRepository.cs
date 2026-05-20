using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Abstractions;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Room> SaveAsync(Room room, CancellationToken ct = default);
}
