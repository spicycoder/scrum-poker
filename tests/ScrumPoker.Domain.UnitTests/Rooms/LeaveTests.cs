using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class LeaveTests
{
    [Fact]
    public void Should_ReturnRoom_WithoutPlayer()
    {
        var (room, _) = Room.Create("Alice");
        var (withBob, _) = room.Join("Bob");

        var (updated, _) = withBob.Leave("Bob");

        updated.Players.ShouldHaveSingleItem();
        updated.Players[0].Name.ShouldBe("Alice");
    }

    [Fact]
    public void Should_ReturnPlayerLeftEvent()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (_, @event) = withBob.Leave("Bob");

        @event.ShouldBeOfType<PlayerLeft>();
        @event.RoomId.ShouldBe(1);
        @event.PlayerName.ShouldBe("Bob");
    }

    [Fact]
    public void Should_Throw_When_PlayerNotInRoom()
    {
        var (room, _) = Room.Create("Alice");

        var ex = Should.Throw<InvalidOperationException>(() => room.Leave("Bob"));
        ex.Message.ShouldContain("Bob");
        ex.Message.ShouldContain("not in room");
    }

    [Fact]
    public void Should_Not_Affect_Other_Players()
    {
        var (room, _) = Room.Create("Alice");
        var (withBob, _) = room.Join("Bob");
        var (voted, _) = withBob.Vote("Alice", "5");

        var (updated, _) = voted.Leave("Bob");

        updated.Players.ShouldHaveSingleItem();
        updated.Players[0].Name.ShouldBe("Alice");
        updated.Players[0].Value.ShouldBe("5");
    }
}
