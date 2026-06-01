using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.API.Features.LeaveRoom;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class LeaveRoomTests
{
    private readonly HttpClient _httpClient;

    public LeaveRoomTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_204NoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), ct);

        var response = await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/leave",
            new LeaveRoomRequest("Bob"), ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_RemovePlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), ct);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/leave",
            new LeaveRoomRequest("Bob"), ct);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}", ct);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(ct);
        gameState.ShouldNotBeNull();
        gameState.Players.ShouldHaveSingleItem();
        gameState.Players.ShouldContainKey("Alice");
    }

    [Fact]
    public async Task With_NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.PostAsJsonAsync("/api/rooms/99999/leave",
            new LeaveRoomRequest("Bob"), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task With_PlayerNotInRoom_Should_Return_404()
    {
        var ct = TestContext.Current.CancellationToken;
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), ct);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var response = await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/leave",
            new LeaveRoomRequest("Bob"), ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
