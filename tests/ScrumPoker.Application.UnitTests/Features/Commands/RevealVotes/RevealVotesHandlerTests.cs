using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Commands.RevealVotes;

public sealed class RevealVotesHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly RevealVotesHandler _sut;

    public RevealVotesHandlerTests()
    {
        _sut = new RevealVotesHandler(_repository);
    }

    [Fact]
    public async Task Handle_Should_ReturnRoomNotFound_When_RoomDoesNotExist()
    {
        var command = new RevealVotesCommand(42);
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<RevealVotesResult.RoomNotFound>();
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

        var result = await _sut.Handle(command, CancellationToken.None);

        var success = result.ShouldBeOfType<RevealVotesResult.Success>();
        success.Room.Revealed.ShouldBeTrue();
        success.Room.Players.Count.ShouldBe(2);

        await _repository.Received(1).SaveAsync(
            Arg.Is<Room>(r => r.Revealed),
            Arg.Any<CancellationToken>());
    }
}
