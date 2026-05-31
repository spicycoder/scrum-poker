using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ScrumPoker.API.Controllers;
using ScrumPoker.Application.Features.Commands.RevealVotes;
using ScrumPoker.Domain.Rooms;
using Wolverine;

namespace ScrumPoker.API.UnitTests.Controllers;

public sealed class RevealVotesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly RoomController _sut;

    public RevealVotesEndpointTests()
    {
        _sut = new RoomController(_bus)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Should_Return_201Created_When_Success()
    {
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RevealVotesResult.Success(new Room()));

        var result = await _sut.Reveal(1, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RevealVotesResult.RoomNotFound());

        var result = await _sut.Reveal(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var result = await _sut.Reveal(id, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<RevealVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<RevealVotesResult>();
        _bus.InvokeAsync<RevealVotesResult>(Arg.Any<RevealVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Reveal(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
