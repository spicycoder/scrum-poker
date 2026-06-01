using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.LeaveRoom;
using ScrumPoker.API.Features.Vote;

namespace ScrumPoker.AppHost.IntegrationTests;

public sealed record GameStateResponse(int GameId, Dictionary<string, string?> Players, bool Revealed);

[Collection(DistributedApplicationCollection.Name)]
public sealed class SignalRTests
{
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(10);

    private readonly DistributedApplicationFixture _fixture;

    public SignalRTests(DistributedApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PlayerJoined_Should_Broadcast_GameState()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("PlayerJoined", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", roomId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), ct);

        var state = await received.Task.WaitAsync(ReceiveTimeout, ct);
        state.GameId.ShouldBe(roomId);
        state.Players.ShouldContainKey("Bob");
    }

    [Fact]
    public async Task VoteCast_Should_Broadcast_GameState()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("VoteCast", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", roomId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "5"), ct);

        var state = await received.Task.WaitAsync(ReceiveTimeout, ct);
        state.GameId.ShouldBe(roomId);
        state.Players["Alice"].ShouldBe("5");
    }

    [Fact]
    public async Task Broadcast_Should_Only_Reach_Members_Of_Room_Group()
    {
        var ct = TestContext.Current.CancellationToken;

        var roomAResp = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomAId = IntegrationTestHelpers.GetRoomIdFromLocation(roomAResp);

        var roomBResp = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Carol"), ct);
        var roomBId = IntegrationTestHelpers.GetRoomIdFromLocation(roomBResp);

        await using var connection = BuildHubConnection();
        var leakReceived = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("PlayerJoined", @event => leakReceived.TrySetResult(@event));

        await connection.StartAsync(ct);
        // Subscribe ONLY to room A. Event will fire in room B.
        await connection.InvokeAsync("JoinRoom", roomAId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomBId}/join",
            new JoinRoomRequest("Dave"), ct);

        var completed = await Task.WhenAny(leakReceived.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
        completed.ShouldNotBe(leakReceived.Task);
    }

    [Fact]
    public async Task LeaveRoom_Should_Stop_Receiving_Group_Broadcasts()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("PlayerJoined", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        var roomKey = roomId.ToString(CultureInfo.InvariantCulture);
        await connection.InvokeAsync("JoinRoom", roomKey, ct);
        await connection.InvokeAsync("LeaveRoom", roomKey, ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), ct);

        var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
        completed.ShouldNotBe(received.Task);
    }

    [Fact]
    public async Task VotesRevealed_Should_Broadcast_GameState()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "5"), ct);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("VotesRevealed", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", roomId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsync($"/api/rooms/{roomId}/reveal", null, ct);

        var state = await received.Task.WaitAsync(ReceiveTimeout, ct);
        state.GameId.ShouldBe(roomId);
        state.Revealed.ShouldBeTrue();
        state.Players["Alice"].ShouldBe("5");
    }

    [Fact]
    public async Task VotesReset_Should_Broadcast_GameState()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "5"), ct);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("VotesReset", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", roomId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsync($"/api/rooms/{roomId}/reset", null, ct);

        var state = await received.Task.WaitAsync(ReceiveTimeout, ct);
        state.GameId.ShouldBe(roomId);
        state.Revealed.ShouldBeFalse();
        state.Players["Alice"].ShouldBeNull();
    }

    [Fact]
    public async Task PlayerLeft_Should_Broadcast_GameState()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _fixture.HttpClient.PostAsJsonAsync($"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), ct);

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<GameStateResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<GameStateResponse>("PlayerLeft", @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", roomId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync($"/api/rooms/{roomId}/leave",
            new LeaveRoomRequest("Bob"), ct);

        var state = await received.Task.WaitAsync(ReceiveTimeout, ct);
        state.GameId.ShouldBe(roomId);
        state.Players.ShouldContainKey("Alice");
        state.Players.ShouldNotContainKey("Bob");
    }

    private HubConnection BuildHubConnection()
    {
        var hubUrl = new UriBuilder(_fixture.HttpClient.BaseAddress!) { Path = "/api/hub" }.Uri;
        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            })
            .Build();
    }
}
