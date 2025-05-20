using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.UnitTests.Builders;
using Microsoft.Extensions.Logging;
using Rag.Chat.Core.UnitTests.Extensions;

namespace Rag.Chat.Core.UnitTests.Services;

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

    [Fact]
    public async Task Query_Should_Return_Concatenated_Response_From_ChatClient()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();
        var mockLogger = new Mock<ILogger<OllamaAiService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var aiServiceOptions = new AiServiceOptionsBuilder().Build();

        var chatMessages = new List<Microsoft.Extensions.AI.ChatResponseUpdate>
        {
            new(ChatRole.Assistant, "Hello"),
            new(ChatRole.Assistant, " world!")
        };

        mockChatClient
            .Setup(c => c.GetStreamingResponseAsync(
                It.IsAny<IReadOnlyList<Microsoft.Extensions.AI.ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelpers.MockAsyncEnumerable<ChatResponseUpdate>(chatMessages));

        var service = new OllamaAiService(
            mockChatClient.Object,
            mockLogger.Object,
            mockConfiguration.Object,
            Options.Create(aiServiceOptions));

        var input = new Models.ChatMessage("Hi");

        // Act
        var result = await service.Query(input);

        // Assert
        result.Should().Be("Hello world!");
    }
}
