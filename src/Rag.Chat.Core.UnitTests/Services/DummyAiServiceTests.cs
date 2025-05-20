namespace Rag.Chat.Core.UnitTests.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Xunit;

public class DummyAiServiceTests
{
    [Fact]
    public async Task Query_Should_Return_HardCoded_Response()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OllamaAiService>>();
        var service = new DummyAiService(mockLogger.Object);
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
        var mockLogger = new Mock<ILogger<OllamaAiService>>();
        var service = new DummyAiService(mockLogger.Object)
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
        tokens.Should().BeEquivalentTo(new[]
        {
            "This",
            " is a",
            " hard-coded",
            " streaming response",
            " from",
            " the AI service.\n"
        });
    }

    [Fact]
    public async Task LoadDocuments_Should_Complete_Without_Exception()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OllamaAiService>>();
        var service = new DummyAiService(mockLogger.Object)
        {
            DelayBetweenMessages = 0 // Set to 0 for faster testing
        };

        // Act
        var act = async () => await service.LoadDocuments();

        // Assert
        await act.Should().NotThrowAsync();
    }
}