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
        var mockConfiguration = new Mock<IConfiguration>();
        var service = new DocumentIngestionService(mockConfiguration.Object, new NullLogger<DocumentIngestionService>());

        // Act
        var act = async () => await service.LoadDocuments();

        // Assert
        await act.Should().NotThrowAsync();
    }
}