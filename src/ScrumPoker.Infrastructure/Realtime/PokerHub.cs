using Microsoft.AspNetCore.SignalR;

namespace ScrumPoker.Infrastructure.Realtime;

public sealed class PokerHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", Context.ConnectionId);
    }

    public async Task SubmitVote(string roomId, string card)
    {
        await Clients.Group(roomId).SendAsync("VoteSubmitted", Context.ConnectionId, card);
    }
}
