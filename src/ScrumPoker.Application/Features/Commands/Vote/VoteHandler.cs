using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<VoteResult> Handle(VoteCommand command, CancellationToken ct)
    {
        var room = await repository.GetByIdAsync(command.RoomId, ct);
        if (room is null)
        {
            return new VoteResult.RoomNotFound();
        }

        try
        {
            var (updated, @event) = room.Vote(command.PlayerName, command.Value);
            var saved = await repository.SaveAsync(updated, ct);
            await bus.PublishAsync(@event);
            return new VoteResult.Success(saved);
        }
        catch (InvalidOperationException)
        {
            return new VoteResult.PlayerNotInRoom();
        }
    }
}
