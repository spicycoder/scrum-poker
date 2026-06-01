using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class ResetVotesTests
{
    [Fact]
    public void Should_ClearAllValues()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");
        var (voted, _) = withBob.Vote("Alice", "5");

        var (reset, _) = voted.ResetVotes();

        reset.Players.Count.ShouldBe(2);
        reset.Players.ShouldAllBe(p => p.Value == null);
    }

    [Fact]
    public void Should_SetRevealedFalse()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };
        var (revealed, _) = roomWithId.Reveal();

        var (reset, _) = revealed.ResetVotes();

        reset.Revealed.ShouldBeFalse();
    }

    [Fact]
    public void Should_ReturnVotesResetEvent()
    {
        var (room, _) = Room.Create("Alice", []);
        var roomWithId = room with { Id = 1 };
        var (voted, _) = roomWithId.Vote("Alice", "5");

        var (_, @event) = voted.ResetVotes();

        @event.ShouldBeOfType<VotesReset>();
        @event.RoomId.ShouldBe(1);
    }
}
