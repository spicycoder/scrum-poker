using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PokerHubEventHandler(IHubContext<PokerHub> hub)
{
    public Task Handle(RoomCreated @event, CancellationToken ct) =>
        hub.Clients.Group(GroupKey(@event.RoomId)).SendAsync(nameof(RoomCreated), @event, ct);

    public Task Handle(PlayerJoined @event, CancellationToken ct) =>
        hub.Clients.Group(GroupKey(@event.RoomId)).SendAsync(nameof(PlayerJoined), @event, ct);

    public Task Handle(VoteCast @event, CancellationToken ct) =>
        hub.Clients.Group(GroupKey(@event.RoomId)).SendAsync(nameof(VoteCast), @event, ct);

    private static string GroupKey(int roomId) => roomId.ToString(CultureInfo.InvariantCulture);
}
