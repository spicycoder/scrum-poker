using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.Vote;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class ResetVotesTests
{
    private readonly HttpClient _httpClient;

    public ResetVotesTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_201Created()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var response = await _httpClient.PostAsync($"/api/rooms/{roomId}/reset",
            null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Should_ClearAllVotes()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "8"), TestContext.Current.CancellationToken);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reset",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Players.ShouldAllBe(p => p.Value == null);
    }

    [Fact]
    public async Task Should_SetRevealedFalse()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reveal",
            null, TestContext.Current.CancellationToken);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reset",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Revealed.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_PersistResetState()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "8"), TestContext.Current.CancellationToken);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reset",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Revealed.ShouldBeFalse();
        gameState.Players.ShouldAllBe(p => p.Value == null);
    }

    [Fact]
    public async Task With_NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.PostAsync("/api/rooms/99999/reset",
            null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
