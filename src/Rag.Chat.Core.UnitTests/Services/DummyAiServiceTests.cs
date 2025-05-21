using Microsoft.Extensions.Logging.Abstractions;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;

namespace Rag.Chat.Core.UnitTests.Services;

public class DummyAiServiceTests
{
    [Fact]
    public async Task Query_Should_Return_HardCoded_Response()
    {
        // Arrange
        var service = new DummyAiService(new NullLogger<DummyAiService>());
        var message = new ChatMessage("Hello");

        // Act
        var result = await service.Query(message);

        // Assert
        result.Should().Be("This is a hard-coded response from the AI service.");
    }

    [Fact]
    public async Task StreamingQuery_Should_Return_Expected_Tokens()
    {
        // Arrange
        var service = new DummyAiService(new NullLogger<DummyAiService>())
        {
            DelayBetweenMessages = 0 // Set to 0 for faster testing
        };

        var message = new ChatMessage("Hello");

        // Act
        var tokens = new List<string>();
        await foreach (var token in service.StreamingQuery(message))
        {
            tokens.Add(token.Content);
        }

        // Assert
        tokens.Should().BeEquivalentTo(
        [
            "This",
            " is a",
            " hard-coded",
            " streaming response",
            " from",
            " the AI service.\n"
        ]);
    }
}