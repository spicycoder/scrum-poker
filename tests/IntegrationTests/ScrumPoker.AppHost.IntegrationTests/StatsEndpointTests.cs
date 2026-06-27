using System.Net;
using System.Net.Http.Json;
using ScrumPoker.API.Features.CreateRoom;
using ScrumPoker.API.Features.JoinRoom;
using ScrumPoker.Domain.Abstractions;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class StatsEndpointTests
{
    private readonly HttpClient _httpClient;

    public StatsEndpointTests(DistributedApplicationFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task GetStats_Should_Have_NonZero_Counts()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice", ["0", "1"]), TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await _httpClient.GetAsync("/api/stats",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<StatsResponse>(
            TestContext.Current.CancellationToken);

        stats.ShouldNotBeNull();
        stats.CurrentMonth.GameCount.ShouldBeGreaterThan(0);
        stats.CurrentMonth.PlayerCount.ShouldBeGreaterThan(0);
    }
}
