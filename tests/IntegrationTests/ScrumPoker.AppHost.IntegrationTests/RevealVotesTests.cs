using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.Vote;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class RevealVotesTests
{
    private readonly HttpClient _httpClient;

    public RevealVotesTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_201Created()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var response = await _httpClient.PostAsync($"/api/rooms/{roomId}/reveal",
            null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Should_SetRevealedTrue()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reveal",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Revealed.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_PersistRevealedState()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reveal",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Revealed.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_KeepVotesVisible()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync($"/api/rooms/{roomId}/vote",
            new VoteRequest("Alice", "8"), TestContext.Current.CancellationToken);

        await _httpClient.PostAsync($"/api/rooms/{roomId}/reveal",
            null, TestContext.Current.CancellationToken);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.Revealed.ShouldBeTrue();
        gameState.Players.ShouldHaveSingleItem();
        gameState.Players["Alice"].ShouldBe("8");
    }

    [Fact]
    public async Task With_NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.PostAsync("/api/rooms/99999/reveal",
            null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
