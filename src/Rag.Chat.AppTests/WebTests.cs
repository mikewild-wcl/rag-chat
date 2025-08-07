using Rag.Chat.Aspire.Shared;

namespace Rag.Chat.AppTests;

public class WebTests
{
    [Fact]
    public async Task GetWebResourceRoot_Returns_OkStatusCode()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Rag_Chat_AppHost>(
            TestContext.Current.CancellationToken);

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var httpClient = app.CreateHttpClient(Services.WebApp);
        await resourceNotificationService
            .WaitForResourceAsync(Services.WebApp, KnownResourceStates.Running, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
