using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class RevealTests
{
    [Fact]
    public void Should_SetRevealedTrue()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };

        var (revealed, _) = roomWithId.Reveal();

        revealed.Revealed.ShouldBeTrue();
    }

    [Fact]
    public void Should_Not_Affect_Players()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };
        var (voted, _) = roomWithId.Vote("Alice", "5");

        var (revealed, _) = voted.Reveal();

        revealed.Players.ShouldHaveSingleItem();
        revealed.Players[0].Value.ShouldBe("5");
    }

    [Fact]
    public void Should_ReturnVotesRevealedEvent()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };

        var (_, @event) = roomWithId.Reveal();

        @event.ShouldBeOfType<VotesRevealed>();
        @event.RoomId.ShouldBe(1);
    }
}
