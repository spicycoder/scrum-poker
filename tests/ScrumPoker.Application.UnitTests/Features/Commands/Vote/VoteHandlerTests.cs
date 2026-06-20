using NSubstitute;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.Vote;

public sealed class VoteHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly VoteHandler _sut;

    public VoteHandlerTests()
    {
        _sut = new VoteHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Room_DoesNotExist()
    {
        var command = new VoteCommand(42, "Bob", "5");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Player_NotInRoom()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new VoteCommand(1, "Bob", "5");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Return_Room_With_UpdatedVote()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new VoteCommand(1, "Alice", "5");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Players.ShouldHaveSingleItem();
        result.Players[0].Name.ShouldBe("Alice");
        result.Players[0].Value.ShouldBe("5");

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Players[0].Value == "5"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Pass_CancellationToken()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", null)]
        };
        var command = new VoteCommand(1, "Alice", "5");
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        await _sut.Handle(command, ct);

        await _repository.Received(1).GetByIdAsync(1, ct);
        await _repository.Received(1).SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), ct);
    }

    [Fact]
    public async Task Handle_Should_Update_Only_Target_Player()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", "3"), new Player("Bob", null)]
        };
        var command = new VoteCommand(1, "Bob", "8");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Players.Count.ShouldBe(2);
        result.Players.ShouldContain(p => p.Name == "Alice" && p.Value == "3");
        result.Players.ShouldContain(p => p.Name == "Bob" && p.Value == "8");
    }

    [Fact]
    public async Task Handle_Should_PublishVoteCastEvent_OnSuccess()
    {
        var room = new Room { Id = 1, Players = [new Player("Alice", null)] };
        var command = new VoteCommand(1, "Alice", "8");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(
            Arg.Is<VoteCast>(e => e.RoomId == 1 && e.PlayerName == "Alice" && e.Value == "8"));
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_RoomNotFound()
    {
        var command = new VoteCommand(42, "Bob", "5");
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<VoteCast>());
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_PlayerNotInRoom()
    {
        var room = new Room { Id = 1, Players = [new Player("Alice", null)] };
        var command = new VoteCommand(1, "Bob", "5");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<VoteCast>());
    }
}
