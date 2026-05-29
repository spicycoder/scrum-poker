using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.Domain.UnitTests.Rooms;

public sealed class RoomTests
{
    [Fact]
    public void Create_Should_ReturnRoom_WithPlayer()
    {
        var (room, _) = Room.Create("Alice");

        room.Players.ShouldHaveSingleItem();
        room.Players[0].Name.ShouldBe("Alice");
        room.Players[0].Value.ShouldBeNull();
    }

    [Fact]
    public void Create_Should_ReturnRoomCreatedEvent()
    {
        var (_, @event) = Room.Create("Alice");

        @event.ShouldBeOfType<RoomCreated>();
        @event.PlayerName.ShouldBe("Alice");
    }

    [Fact]
    public void Join_Should_ReturnRoom_WithNewPlayer()
    {
        var (room, _) = Room.Create("Alice");

        var (updated, _) = room.Join("Bob");

        updated.Players.Count.ShouldBe(2);
        updated.Players.ShouldContain(p => p.Name == "Alice");
        updated.Players.ShouldContain(p => p.Name == "Bob" && p.Value == null);
    }

    [Fact]
    public void Join_Should_ReturnPlayerJoinedEvent()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var (_, @event) = roomWithId.Join("Bob");

        @event.ShouldBeOfType<PlayerJoined>();
        @event.RoomId.ShouldBe(1);
        @event.PlayerName.ShouldBe("Bob");
    }

    [Fact]
    public void Join_Should_Throw_When_PlayerAlreadyInRoom()
    {
        var (room, _) = Room.Create("Alice");

        var ex = Should.Throw<InvalidOperationException>(() => room.Join("Alice"));
        ex.Message.ShouldContain("Alice");
        ex.Message.ShouldContain("already in room");
    }

    [Fact]
    public void Vote_Should_UpdatePlayerValue()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var (updated, _) = roomWithId.Vote("Alice", "5");

        updated.Players.ShouldHaveSingleItem();
        updated.Players[0].Value.ShouldBe("5");
    }

    [Fact]
    public void Vote_Should_ReturnVoteCastEvent()
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
    public void Vote_Should_Throw_When_PlayerNotInRoom()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var ex = Should.Throw<InvalidOperationException>(() => roomWithId.Vote("Bob", "5"));
        ex.Message.ShouldContain("Bob");
        ex.Message.ShouldContain("not in room");
    }

    [Fact]
    public void Vote_Should_Not_Affect_Other_Players()
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
    public void Vote_Should_AutoReveal_When_AllPlayersVoted()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (afterAlice, _) = withBob.Vote("Alice", "5");
        var (afterBob, _) = afterAlice.Vote("Bob", "8");

        afterBob.Revealed.ShouldBeTrue();
    }

    [Fact]
    public void Vote_Should_NotReveal_When_NotAllPlayersVoted()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (withBob, _) = roomWithId.Join("Bob");

        var (updated, _) = withBob.Vote("Alice", "5");

        updated.Revealed.ShouldBeFalse();
    }

    [Fact]
    public void Reveal_Should_SetRevealedTrue()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };

        var revealed = roomWithId.Reveal();

        revealed.Revealed.ShouldBeTrue();
    }

    [Fact]
    public void Reveal_Should_Not_Affect_Players()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var (voted, _) = roomWithId.Vote("Alice", "5");

        var revealed = voted.Reveal();

        revealed.Players.ShouldHaveSingleItem();
        revealed.Players[0].Value.ShouldBe("5");
    }

    [Fact]
    public void ResetVotes_Should_ClearAllValues()
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
    public void ResetVotes_Should_SetRevealedFalse()
    {
        var (room, _) = Room.Create("Alice");
        var roomWithId = room with { Id = 1 };
        var revealed = roomWithId.Reveal();

        var reset = revealed.ResetVotes();

        reset.Revealed.ShouldBeFalse();
    }
}
