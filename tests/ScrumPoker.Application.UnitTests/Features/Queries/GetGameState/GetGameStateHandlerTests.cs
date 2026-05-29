using NSubstitute;
using ScrumPoker.Application.Abstractions;
using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.Application.Features.Queries.GetGameState;

public sealed class GetGameStateHandlerTests
{
    private readonly IRoomRepository _repository = Substitute.For<IRoomRepository>();
    private readonly GetGameStateHandler _sut;

    public GetGameStateHandlerTests()
    {
        _sut = new GetGameStateHandler(_repository);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RoomExists()
    {
        var room = new Room { Id = 42, Players = [new Player("Alice", null)] };
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(room);

        var result = await _sut.Handle(new GetGameStateQuery(42), CancellationToken.None);

        var success = result.ShouldBeOfType<GetGameStateResult.Success>();
        success.Room.Id.ShouldBe(42);
        success.Room.Players.ShouldHaveSingleItem();
        success.Room.Players[0].Name.ShouldBe("Alice");
    }

    [Fact]
    public async Task Handle_Should_ReturnRoomNotFound_When_RoomDoesNotExist()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var result = await _sut.Handle(new GetGameStateQuery(99), CancellationToken.None);

        result.ShouldBeOfType<GetGameStateResult.RoomNotFound>();
    }
}
