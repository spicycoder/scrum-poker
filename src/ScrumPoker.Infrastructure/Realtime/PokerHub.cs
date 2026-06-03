using Microsoft.AspNetCore.SignalR;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PokerHub(PlayerConnectionTracker tracker) : Hub
{
    private const string RoomIdKey = "roomId";
    private const string PlayerNameKey = "playerName";

    public async Task JoinRoom(string roomId, string playerName)
    {
        if (int.TryParse(roomId, out var id))
        {
            Context.Items[RoomIdKey] = id;
            Context.Items[PlayerNameKey] = playerName;
            await tracker.Join(id, playerName, Context.ConnectionId);
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public Task LeaveRoom(string roomId)
    {
        if (Context.Items.TryGetValue(RoomIdKey, out var roomIdObj) && roomIdObj is int id &&
            Context.Items.TryGetValue(PlayerNameKey, out var playerNameObj) && playerNameObj is string playerName)
        {
            tracker.Leave(id, playerName, Context.ConnectionId);
        }
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(RoomIdKey, out var roomIdObj) && roomIdObj is int roomId &&
            Context.Items.TryGetValue(PlayerNameKey, out var playerNameObj) && playerNameObj is string playerName)
        {
            tracker.Leave(roomId, playerName, Context.ConnectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }
}
