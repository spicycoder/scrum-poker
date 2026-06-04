using NSubstitute;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Domain.Rooms;
using ScrumPoker.Domain.Rooms.Events;
using Wolverine;

namespace ScrumPoker.Application.Features.Commands.ResetVotes;

public sealed class ResetVotesHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly ResetVotesHandler _sut;

    public ResetVotesHandlerTests()
    {
        _sut = new ResetVotesHandler(_repository, _bus);
    }

    [Fact]
    public async Task Handle_Should_ReturnRoomNotFound_When_RoomDoesNotExist()
    {
        var command = new ResetVotesCommand(42);
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<ResetVotesResult.RoomNotFound>();
    }

    [Fact]
    public async Task Handle_Should_ClearAllVotes_And_SetRevealedFalse()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", "5"), new Player("Bob", "8")],
            Revealed = true
        };
        var command = new ResetVotesCommand(1);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(command, CancellationToken.None);

        var success = result.ShouldBeOfType<ResetVotesResult.Success>();
        success.Room.Revealed.ShouldBeFalse();
        success.Room.Players.ShouldAllBe(p => p.Value == null);

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => !r.Revealed && r.Players.All(p => p.Value == null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishVotesResetEvent_OnSuccess()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Alice", "5")],
            Revealed = true
        };
        var command = new ResetVotesCommand(1);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(room);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.Received(1).PublishAsync(Arg.Is<VotesReset>(e => e.RoomId == 1));
    }

    [Fact]
    public async Task Handle_Should_NotPublishEvent_When_RoomNotFound()
    {
        var command = new ResetVotesCommand(42);
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        await _sut.Handle(command, CancellationToken.None);

        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<VotesReset>());
    }
}
