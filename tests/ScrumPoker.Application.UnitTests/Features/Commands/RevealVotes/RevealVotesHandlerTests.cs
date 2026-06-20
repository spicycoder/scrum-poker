using NSubstitute;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public sealed class RevealVotesHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RevealVotesHandler _sut;

    public RevealVotesHandlerTests()
    {
        _sut = new RevealVotesHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Room_DoesNotExist()
    {
        var command = new RevealVotesCommand(42);
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_Should_SetRevealedTrue()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", "5"), new Player("Bob", "8")],
            Revealed = false
        };
        var command = new RevealVotesCommand(1);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Revealed.ShouldBeTrue();
        result.Players.Count.ShouldBe(2);

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Revealed),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishVotesRevealedEvent_OnSuccess()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", "5")],
            Revealed = false
        };
        var command = new RevealVotesCommand(1);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);
        _repository.SaveAsync(Arg.Any<Room>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Room>(0));

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(Arg.Is<VotesRevealed>(e => e.RoomId == 1));
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_RoomNotFound()
    {
        var command = new RevealVotesCommand(42);
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<VotesRevealed>());
    }
}
