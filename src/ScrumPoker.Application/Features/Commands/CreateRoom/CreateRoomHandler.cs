using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.CreateRoom;

public sealed class CreateRoomHandler(IRoomRepository repository, IMessageBus bus)
{
    public async Task<Room> Handle(CreateRoomCommand command, CancellationToken ct)
    {
        var (room, @event) = Room.Create(command.PlayerName, command.CardSet);
        var warmupRoom = room with { IsWarmup = command.IsWarmup };
        var saved = await repository.SaveAsync(warmupRoom, command.Expiry, ct);
        await bus.PublishAsync(@event with { RoomId = saved.Id });

        return saved;
    }
}
