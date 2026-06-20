using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Queries.GetGameState;

public sealed class GetGameStateHandler(IRoomRepository repository)
{
    public async Task<Room?> Handle(GetGameStateQuery query, CancellationToken ct)
    {
        return await repository.GetByIdAsync(query.RoomId, ct);
    }
}
