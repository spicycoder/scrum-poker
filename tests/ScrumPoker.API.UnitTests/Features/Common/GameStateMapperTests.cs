using ScrumPoker.Domain.Rooms;

namespace ScrumPoker.API.Features.Common;

public sealed class GameStateMapperTests
{
    [Fact]
    public void ToResponse_Should_Map_All_Properties()
    {
        var room = new Room
        {
            Id = 42,
            Players =
            [
                new Player("Alice", "5"),
                new Player("Bob", "8")
            ]
        };

        var response = GameStateMapper.ToResponse(room);

        response.GameId.ShouldBe(42);
        response.Players.Count.ShouldBe(2);

        response.Players[0].Name.ShouldBe("Alice");
        response.Players[0].Value.ShouldBe("5");
        response.Players[1].Name.ShouldBe("Bob");
        response.Players[1].Value.ShouldBe("8");
    }

    [Fact]
    public void ToResponse_Should_Map_Null_PlayerValue()
    {
        var room = new Room
        {
            Id = 1,
            Players = [new Player("Charlie", null)]
        };

        var response = GameStateMapper.ToResponse(room);

        response.Players[0].Name.ShouldBe("Charlie");
        response.Players[0].Value.ShouldBeNull();
    }

    [Fact]
    public void ToResponse_Should_Handle_Empty_Players()
    {
        var room = new Room
        {
            Id = 99,
            Players = []
        };

        var response = GameStateMapper.ToResponse(room);

        response.GameId.ShouldBe(99);
        response.Players.ShouldBeEmpty();
    }
}
