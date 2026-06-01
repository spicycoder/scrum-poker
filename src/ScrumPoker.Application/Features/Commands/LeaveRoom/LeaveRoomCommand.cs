namespace ScrumPoker.Application.Features.Commands.LeaveRoom;

public sealed record LeaveRoomCommand(int RoomId, string PlayerName);
