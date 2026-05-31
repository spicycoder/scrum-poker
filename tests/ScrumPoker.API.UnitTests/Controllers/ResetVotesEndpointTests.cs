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
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ResetVotesResult.Success(new Room()));

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        var statusCode = result.ShouldBeOfType<StatusCodeResult>();
        statusCode.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task Should_Return_404NotFound_When_RoomNotFound()
    {
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ResetVotesResult.RoomNotFound());

        var result = await _sut.Reset(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_Return_400BadRequest_When_IdIsInvalid(int id)
    {
        var result = await _sut.Reset(id, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<BadRequestResult>();
        await _bus.DidNotReceiveWithAnyArgs().InvokeAsync<ResetVotesResult>(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Should_Throw_InvalidOperationException_For_UnknownResultType()
    {
        var unknownResult = Substitute.For<ResetVotesResult>();
        _bus.InvokeAsync<ResetVotesResult>(Arg.Any<ResetVotesCommand>(), Arg.Any<CancellationToken>())
            .Returns(unknownResult);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Reset(1, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Unknown result type");
    }
}
