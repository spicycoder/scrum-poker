using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PokerHubEventHandler(IHubContext<PokerHub> hub)
{
    public Task Handle(RoomCreated @event, CancellationToken ct) => BroadcastGameState(nameof(RoomCreated), @event, ct);
    public Task Handle(PlayerJoined @event, CancellationToken ct) => BroadcastGameState(nameof(PlayerJoined), @event, ct);
    public Task Handle(VoteCast @event, CancellationToken ct) => BroadcastGameState(nameof(VoteCast), @event, ct);
    public Task Handle(VotesRevealed @event, CancellationToken ct) => BroadcastGameState(nameof(VotesRevealed), @event, ct);
    public Task Handle(VotesReset @event, CancellationToken ct) => BroadcastGameState(nameof(VotesReset), @event, ct);
    public Task Handle(PlayerLeft @event, CancellationToken ct) => BroadcastGameState(nameof(PlayerLeft), @event, ct);

    private Task BroadcastGameState(string eventName, IRoomState @event, CancellationToken ct)
    {
        var room = @event.Room;
        var response = new
        {
            gameId = room.Id,
            players = room.Players.ToDictionary(p => p.Name, p => p.Value),
            revealed = room.Revealed
        };
        return hub.Clients.Group(GroupKey(room.Id)).SendAsync(eventName, response, ct);
    }

    private static string GroupKey(int roomId) => roomId.ToString(CultureInfo.InvariantCulture);
}
