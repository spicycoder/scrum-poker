using System.Net;
using System.Net.Http.Json;
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
    public async Task Should_Return_201Created_When_Success()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task NonExistentRoom_Should_Return_404()
    {
        var response = await _httpClient.PostAsJsonAsync("/api/rooms/9999/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WithDuplicateName_Should_Return_409()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Alice"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Before_Expiry_Room_Should_Be_Accessible()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Charlie"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
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
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        // Join resets the TTL back to 4s.
        await fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        // t≈5s: past the original expiry (t=4s), but within the reset window (t=6s).
        await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);

        var joinResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Charlie"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    // No action after creation — room must expire naturally.
    [Fact]
    public async Task After_Expiry_Room_Should_Return_404()
    {
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var joinResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
