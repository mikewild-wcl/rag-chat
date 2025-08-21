using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.UnitTests.Builders;

namespace Rag.Chat.Core.UnitTests.Services;

public class DummyAiServiceTests
{
    [Fact]
    public void ClearChat_Should_Clear_Conversation()
    {
        // Arrange
        const string userId = "User123";

        var service = AiServiceBuilder.BuildDummyService();

        // Act
        var taskResult = service.ClearChat(userId);

        // Assert
        taskResult.Should().Be(Task.CompletedTask);
    }

    [Fact]
    public async Task Query_Should_Return_HardCoded_Response()
    {
        // Arrange
        var service = AiServiceBuilder.BuildDummyService();
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
        var options = AiServiceOptionsBuilder
            .Build(messageDelayInMilliseconds: 0);// Set to 0 for faster testing

        var service = new DummyAiService(
             Options.Create(options),
             new NullLogger<DummyAiService>());

        var message = new ChatMessage("Hello");

        // Act
        var tokens = new List<string>();
        await foreach (var token in service.StreamingQuery(message))
        {
            tokens.Add(token.Content);
        }

        // Assert
        tokens.Should().BeEquivalentTo(new List<string>
        {
            "This",
            " is a",
            " hard-coded",
            " streaming response",
            " from",
            " the AI service.\n",
            "\n",
            " And here is",
            " another",
            " paragraph.\n",
        });
    }
}