using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class JoinTests
{
    [Fact]
    public void Should_ReturnRoom_WithNewPlayer()
    {
        var (room, _) = Room.Create("Alice");

        var (updated, _) = room.Join("Bob");

        updated.Players.Count.ShouldBe(2);
        updated.Players.ShouldContain(p => p.Name == "Alice");
        updated.Players.ShouldContain(p => p.Name == "Bob" && p.Value == null);
    }

    [Fact]
    public void Should_ReturnPlayerJoinedEvent()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var (_, @event) = roomWithId.Join("Bob");

        @event.ShouldBeOfType<PlayerJoined>();
        @event.RoomId.ShouldBe(1);
        @event.PlayerName.ShouldBe("Bob");
    }

    [Fact]
    public void Should_Throw_When_PlayerAlreadyInRoom()
    {
        var (room, _) = Room.Create("Alice");

        var ex = Should.Throw<InvalidOperationException>(() => room.Join("Alice"));
        ex.Message.ShouldContain("Alice");
        ex.Message.ShouldContain("already in room");
    }
}
