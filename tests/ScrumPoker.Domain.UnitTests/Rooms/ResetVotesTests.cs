using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class ResetVotesTests
{
    [Fact]
    public void Should_ClearAllValues()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");
        var (voted, _) = withBob.Vote("Alice", "5");

        var reset = voted.ResetVotes();

        reset.Players.Count.ShouldBe(2);
        reset.Players.ShouldAllBe(p => p.Value == null);
    }

    [Fact]
    public void Should_SetRevealedFalse()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var revealed = roomWithId.Reveal();

        var reset = revealed.ResetVotes();

        reset.Revealed.ShouldBeFalse();
    }
}
