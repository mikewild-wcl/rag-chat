using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.Tests.Builders;

namespace Rag.Chat.Core.Tests.Services;

public class OllamaAiServiceTests
{
    [Fact]
    public void Constructor_Should_Not_Throw_When_Valid_Parameters()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<OllamaAiService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockAiServiceOptions = new Mock<IOptions<AiServiceOptions>>();

        var aiServiceOptions = new AiServiceOptionsBuilder().Build();

        // Act
        var act = () => new OllamaAiService(
            mockChatClient.Object,
            mockLogger.Object,
            mockConfiguration.Object,
            Options.Create(aiServiceOptions));

        // Assert
        act.Should().NotThrow();
    }
}
