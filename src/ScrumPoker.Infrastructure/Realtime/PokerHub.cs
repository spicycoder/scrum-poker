using Microsoft.AspNetCore.SignalR;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PokerHub : Hub
{
    public Task JoinRoom(string roomId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, roomId);

    public Task LeaveRoom(string roomId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
}
