using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ScrumPoker.API.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return_True()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var result = await _sut.TryHandleAsync(httpContext, new Exception("test"), TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_Should_Set_StatusCode_500()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await _sut.TryHandleAsync(httpContext, new Exception("test"), TestContext.Current.CancellationToken);

        httpContext.Response.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Write_ProblemDetails_Json()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await _sut.TryHandleAsync(httpContext, new Exception("test"), TestContext.Current.CancellationToken);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);

        var problem = JsonSerializer.Deserialize<JsonElement>(body);
        problem.GetProperty("status").GetInt32().ShouldBe(500);
        problem.GetProperty("title").GetString().ShouldBe("Server error");
    }

    [Fact]
    public async Task TryHandleAsync_Should_Log_Error()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Something broke");

        await _sut.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TryHandleAsync_Should_Respond_Json_ContentType()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await _sut.TryHandleAsync(httpContext, new Exception("test"), TestContext.Current.CancellationToken);

        httpContext.Response.ContentType.ShouldContain("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return_400_For_ValidationException()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var failures = new List<ValidationFailure>
        {
            new("PlayerName", "Player name must not be empty"),
            new("PlayerName", "Player name must not exceed 50 characters")
        };
        var exception = new ValidationException(failures);

        await _sut.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        httpContext.Response.StatusCode.ShouldBe(400);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Write_ValidationErrors_In_Response()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var failures = new List<ValidationFailure>
        {
            new("PlayerName", "Player name must not be empty"),
            new("RoomId", "Room ID must be greater than 0")
        };
        var exception = new ValidationException(failures);

        await _sut.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);

        var problem = JsonSerializer.Deserialize<JsonElement>(body);
        problem.GetProperty("status").GetInt32().ShouldBe(400);
        problem.GetProperty("title").GetString().ShouldBe("Validation error");

        var errors = problem.GetProperty("errors");
        errors.GetProperty("PlayerName").EnumerateArray().Select(e => e.GetString()).ShouldContain("Player name must not be empty");
        errors.GetProperty("RoomId").EnumerateArray().Select(e => e.GetString()).ShouldContain("Room ID must be greater than 0");
    }
}
