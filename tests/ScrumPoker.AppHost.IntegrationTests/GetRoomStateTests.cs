using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class GetRoomStateTests
{
    private readonly HttpClient _httpClient;

    public GetRoomStateTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_200_With_GameStateResponse()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var getResponse = await _httpClient.GetAsync($"/api/rooms/{roomId}",
            TestContext.Current.CancellationToken);

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var gameState = await getResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        gameState.ShouldNotBeNull();
        gameState.GameId.ShouldBe(roomId);
        gameState.Players.ShouldHaveSingleItem();
        gameState.Players.ShouldContainKey("Alice");
    }

    [Fact]
    public async Task With_NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.GetAsync("/api/rooms/99999",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task With_InvalidId_Should_Return_400()
    {
        var response = await _httpClient.GetAsync("/api/rooms/0",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
