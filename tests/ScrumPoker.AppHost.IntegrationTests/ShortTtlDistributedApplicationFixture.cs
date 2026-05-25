using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace ScrumPoker.AppHost.IntegrationTests;

public sealed class ShortTtlDistributedApplicationFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    private DistributedApplication _app = null!;

    public HttpClient HttpClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var ct = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ScrumPoker_AppHost>(ct);

        var api = appHost.CreateResourceBuilder<ProjectResource>("scrumpoker-api");
        api.WithEnvironment("Game__ExpirationSeconds", "4");

        _app = await appHost.BuildAsync(ct).WaitAsync(ct);
        await _app.StartAsync(ct).WaitAsync(ct);

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("scrumpoker-api", ct)
            .WaitAsync(ct);

        HttpClient = _app.CreateHttpClient("scrumpoker-api", new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
