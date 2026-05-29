using ScrumPoker.Application.Abstractions;

namespace ScrumPoker.Application.Features.Queries.GetGameState;

public sealed class GetGameStateHandler(IRoomRepository repository)
{
    public async Task<GetGameStateResult> Handle(GetGameStateQuery query, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(query.RoomId, ct);
        return room is null
            ? new GetGameStateResult.RoomNotFound()
            : new GetGameStateResult.Success(room);
    }
}
