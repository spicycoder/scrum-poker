using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.Application.Features.Commands.ResetVotes;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class ResetVotesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public ResetVotesEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_201Created_When_Success()
    {
        _bus.InvokeAsync<Room?>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new Room());

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<Room?>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

}
