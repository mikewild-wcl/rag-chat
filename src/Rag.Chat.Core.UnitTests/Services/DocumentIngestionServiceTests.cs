using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Rag.Chat.Core.Services;

namespace Rag.Chat.Core.UnitTests.Services;

public class DocumentIngestionServiceTests
{
    [Fact]
    public async Task LoadDocuments_Should_Complete_Without_Exception()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            { "SourceDocuments:0:Title", "page1" },
            { "SourceDocuments:0:SourceUri", "https://abc.ai" },
            { "SourceDocuments:0:Type", "html" },
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new DocumentIngestionService(configuration, new NullLogger<DocumentIngestionService>());

        // Act
        var act = async () => await service.LoadDocuments();

        // Assert
        await act.Should().NotThrowAsync();
    }
}