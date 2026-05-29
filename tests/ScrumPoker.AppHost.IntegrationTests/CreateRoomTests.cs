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
    public async Task Should_Return_201Created()
    {
        var request = new CreateRoomRequest("Alice");

        var response = await _httpClient.PostAsJsonAsync("/api/rooms", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task With_EmptyName_Should_Return_400()
    {
        var request = new CreateRoomRequest("");

        var response = await _httpClient.PostAsJsonAsync("/api/rooms", request,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Before_Expiry_Room_Should_Be_Accessible()
    {
        var createResponse = await _httpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        var joinResponse = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}

public sealed class CreateRoomExpiryTests(ShortTtlDistributedApplicationFixture fixture)
    : IClassFixture<ShortTtlDistributedApplicationFixture>
{
    [Fact]
    public async Task After_Expiry_Room_Should_Return_404()
    {
        var createResponse = await fixture.HttpClient.PostAsJsonAsync("/api/rooms",
            new CreateRoomRequest("Alice"), TestContext.Current.CancellationToken);
        var roomId = IntegrationTestHelpers.GetRoomIdFromLocation(createResponse);

        await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var joinResponse = await fixture.HttpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/join",
            new JoinRoomRequest("Bob"), TestContext.Current.CancellationToken);

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
