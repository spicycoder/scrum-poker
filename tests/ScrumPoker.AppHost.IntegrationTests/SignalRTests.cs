using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.Vote;
using ScrumPoker.Domain.Rooms.Events;

namespace ScrumPoker.AppHost.IntegrationTests;

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
    public async Task PlayerJoined_Should_Broadcast_To_Group()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>(ct);
        created.ShouldNotBeNull();

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<PlayerJoined>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<PlayerJoined>(nameof(PlayerJoined), @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", created.GameId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), ct);

        var @event = await received.Task.WaitAsync(ReceiveTimeout, ct);
        @event.RoomId.ShouldBe(created.GameId);
        @event.PlayerName.ShouldBe("Bob");
    }

    [Fact]
    public async Task VoteCast_Should_Broadcast_To_Group()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>(ct);
        created.ShouldNotBeNull();

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<VoteCast>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<VoteCast>(nameof(VoteCast), @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        await connection.InvokeAsync("JoinRoom", created.GameId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/vote",
            new VoteRequest("Alice", "5"), ct);

        var @event = await received.Task.WaitAsync(ReceiveTimeout, ct);
        @event.RoomId.ShouldBe(created.GameId);
        @event.PlayerName.ShouldBe("Alice");
        @event.Value.ShouldBe("5");
    }

    [Fact]
    public async Task Broadcast_Should_Only_Reach_Members_Of_Room_Group()
    {
        var ct = TestContext.Current.CancellationToken;

        var roomAResp = await _fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomA = await roomAResp.Content.ReadFromJsonAsync<GameStateResponse>(ct);
        roomA.ShouldNotBeNull();

        var roomBResp = await _fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Carol"), ct);
        var roomB = await roomBResp.Content.ReadFromJsonAsync<GameStateResponse>(ct);
        roomB.ShouldNotBeNull();

        await using var connection = BuildHubConnection();
        var leakReceived = new TaskCompletionSource<PlayerJoined>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<PlayerJoined>(nameof(PlayerJoined), @event => leakReceived.TrySetResult(@event));

        await connection.StartAsync(ct);
        // Subscribe ONLY to room A. Event will fire in room B.
        await connection.InvokeAsync("JoinRoom", roomA.GameId.ToString(CultureInfo.InvariantCulture), ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{roomB.GameId}/join",
            new JoinRoomRequest("Dave"), ct);

        var completed = await Task.WhenAny(leakReceived.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
        completed.ShouldNotBe(leakReceived.Task);
    }

    [Fact]
    public async Task LeaveRoom_Should_Stop_Receiving_Group_Broadcasts()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<GameStateResponse>(ct);
        created.ShouldNotBeNull();

        await using var connection = BuildHubConnection();
        var received = new TaskCompletionSource<PlayerJoined>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<PlayerJoined>(nameof(PlayerJoined), @event => received.TrySetResult(@event));

        await connection.StartAsync(ct);
        var roomKey = created.GameId.ToString(CultureInfo.InvariantCulture);
        await connection.InvokeAsync("JoinRoom", roomKey, ct);
        await connection.InvokeAsync("LeaveRoom", roomKey, ct);

        await _fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), ct);

        var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
        completed.ShouldNotBe(received.Task);
    }

    private HubConnection BuildHubConnection()
    {
        var hubUrl = new UriBuilder(_fixture.HttpClient.BaseAddress!) { Path = "/hub" }.Uri;
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
