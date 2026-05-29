using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Commands.ResetVotes;

public sealed class ResetVotesHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly ResetVotesHandler _sut;

    public ResetVotesHandlerTests()
    {
        _sut = new ResetVotesHandler(_repository);
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
}
