using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class CreateRoomTests
{
    private readonly HttpClient _httpClient;

    public CreateRoomTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_201_With_GameStateResponse()
    {
        var request = new CreateRoomRequest("Alice");

        var response = await _httpClient.PostAsJsonAsync("/rooms", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var gameState = await response.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);

        gameState.ShouldNotBeNull();
        gameState.GameId.ShouldBeGreaterThan(0);
        gameState.Players.ShouldHaveSingleItem();
        gameState.Players[0].Name.ShouldBe("Alice");
        gameState.Players[0].Value.ShouldBeNull();
    }

    [Fact]
    public async Task With_EmptyName_Should_Return_400()
    {
        var request = new CreateRoomRequest("");

        var response = await _httpClient.PostAsJsonAsync("/rooms", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Before_Expiry_Room_Should_Be_Accessible()
    {
        var request = new CreateRoomRequest("Alice");

        var createResponse = await _httpClient.PostAsJsonAsync("/rooms", request,
            TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public sealed class CreateRoomExpiryTests(ShortTtlDistributedApplicationFixture fixture)
    : IClassFixture<ShortTtlDistributedApplicationFixture>
{
    [Fact]
    public async Task After_Expiry_Room_Should_Return_404()
    {
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var joinResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
