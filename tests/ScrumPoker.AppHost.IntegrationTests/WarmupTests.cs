using System.Net;
using System.Net.Http.Json;

namespace ScrumPoker.AppHost.IntegrationTests;

[Collection(WarmupCollection.Name)]
public sealed class WarmupTests
{
    private readonly HttpClient _httpClient;

    public WarmupTests(WarmupFixture fixture)
    {
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task Warmup_Should_Return_200()
    {
        var response = await _httpClient.GetAsync("/api/warmup", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Room_Should_Expire_After_Ttl()
    {
        var warmupResponse = await _httpClient.GetAsync(
            "/api/warmup", TestContext.Current.CancellationToken);
        warmupResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var roomId = await warmupResponse.Content.ReadFromJsonAsync<int>();

        var getResponse = await _httpClient.GetAsync(
            $"/api/rooms/{roomId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        await Task.Delay(TimeSpan.FromSeconds(7));

        var expiredResponse = await _httpClient.GetAsync(
            $"/api/rooms/{roomId}", TestContext.Current.CancellationToken);
        expiredResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
