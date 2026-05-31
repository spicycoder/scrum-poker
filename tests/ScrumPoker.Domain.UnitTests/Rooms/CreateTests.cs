using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class CreateTests
{
    [Fact]
    public void Should_ReturnRoom_WithPlayer()
    {
        var (room, _) = Room.Create("Alice");

        room.Players.ShouldHaveSingleItem();
        room.Players[0].Name.ShouldBe("Alice");
        room.Players[0].Value.ShouldBeNull();
    }

    [Fact]
    public void Should_ReturnRoomCreatedEvent()
    {
        var (_, @event) = Room.Create("Alice");

        @event.ShouldBeOfType<RoomCreated>();
        @event.PlayerName.ShouldBe("Alice");
    }
}
