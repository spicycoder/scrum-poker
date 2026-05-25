using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.Common;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class JoinRoomTests
{
    private readonly HttpClient _httpClient;

    public JoinRoomTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Should_Return_200_With_UpdatedGameState()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var joined = await joinResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        joined.ShouldNotBeNull();
        joined.Players.Count.ShouldBe(2);
    }

    [Fact]
    public async Task NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.PostAsJsonAsync("/rooms/9999/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WithDuplicateName_Should_Return_409()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Alice"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Before_Expiry_Room_Should_Be_Accessible()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        await _httpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Charlie"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public sealed class JoinRoomExpiryTests(ShortTtlDistributedApplicationFixture fixture)
    : IClassFixture<ShortTtlDistributedApplicationFixture>
{
    // TTL = 4s. Join at t≈2s resets it to 4s (expires at t≈6s).
    // At t≈5s the room would have been gone without the reset — proves timer was restarted.
    [Fact]
    public async Task Join_Should_Reset_Expiry_Timer()
    {
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var created = await createResponse.Content
            .ReadFromJsonAsync<GameStateResponse>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();

        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        // Join resets the TTL back to 4s.
        await fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        // t≈5s: past the original expiry (t=4s), but within the reset window (t=6s).
        await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);

        var joinResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/rooms/{created.GameId}/join",
            new JoinRoomRequest("Charlie"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // No action after creation — room must expire naturally.
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
