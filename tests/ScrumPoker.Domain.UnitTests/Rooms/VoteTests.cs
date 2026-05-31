using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class VoteTests
{
    [Fact]
    public void Should_UpdatePlayerValue()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var (updated, _) = roomWithId.Vote("Alice", "5");

        updated.Players.ShouldHaveSingleItem();
        updated.Players[0].Value.ShouldBe("5");
    }

    [Fact]
    public void Should_ReturnVoteCastEvent()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var (_, @event) = roomWithId.Vote("Alice", "8");

        @event.ShouldBeOfType<VoteCast>();
        @event.RoomId.ShouldBe(1);
        @event.PlayerName.ShouldBe("Alice");
        @event.Value.ShouldBe("8");
    }

    [Fact]
    public void Should_Throw_When_PlayerNotInRoom()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var ex = Should.Throw<InvalidOperationException>(() => roomWithId.Vote("Bob", "5"));
        ex.Message.ShouldContain("Bob");
        ex.Message.ShouldContain("not in room");
    }

    [Fact]
    public void Should_Not_Affect_Other_Players()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (updated, _) = withBob.Vote("Alice", "5");

        updated.Players.Count.ShouldBe(2);
        updated.Players.ShouldContain(p => p.Name == "Alice" && p.Value == "5");
        updated.Players.ShouldContain(p => p.Name == "Bob" && p.Value == null);
    }

    [Fact]
    public void Should_AutoReveal_When_AllPlayersVoted()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (afterAlice, _) = withBob.Vote("Alice", "5");
        var (afterBob, _) = afterAlice.Vote("Bob", "8");

        afterBob.Revealed.ShouldBeTrue();
    }

    [Fact]
    public void Should_NotReveal_When_NotAllPlayersVoted()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (updated, _) = withBob.Vote("Alice", "5");

        updated.Revealed.ShouldBeFalse();
    }
}
