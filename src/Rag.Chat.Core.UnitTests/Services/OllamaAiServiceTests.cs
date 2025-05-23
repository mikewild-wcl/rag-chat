using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services;
using System.Text;

namespace Rag.Chat.Core.UnitTests.Services;

public class OllamaAiServiceTests
{
    [Fact]
    public void Constructor_Should_Not_Throw_When_Valid_Parameters()
    {
        // Arrange
        var kernel = new Kernel();

        // Act
        var act = () => new OllamaAiService(
            kernel,
            new NullLogger<OllamaAiService>());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Query_Should_Return_Concatenated_Response_From_ChatClient()
    {
        // Arrange
        var mockChatCompletion = new Mock<IChatCompletionService>();
        mockChatCompletion
            .Setup(x => x.GetStreamingChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .Returns(new List<StreamingChatMessageContent>()
            {
                new(AuthorRole.Assistant, "Hello world!")
            }
            .ToAsyncEnumerable());

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(mockChatCompletion.Object);

        var kernel = kernelBuilder.Build();

        var service = new OllamaAiService(
            kernel,
            new NullLogger<OllamaAiService>());

        var input = new Models.ChatMessage("Hi");

        // Act
        var result = await service.Query(input);

        // Assert
        result.Should().Be("Hello world!");
    }

    [Fact]
    public async Task StreamingQuery_Should_Return_Concatenated_Response_From_ChatClient()
    {
        // Arrange
        var mockChatCompletion = new Mock<IChatCompletionService>();
        mockChatCompletion
            .Setup(x => x.GetStreamingChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .Returns(new List<StreamingChatMessageContent>()
            {
                new(AuthorRole.Assistant, "Hello"),
                new(AuthorRole.Assistant, " "),
                new(AuthorRole.Assistant, "world"),
                new(AuthorRole.Assistant, "!")
            }
            .ToAsyncEnumerable());

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(mockChatCompletion.Object);

        var kernel = kernelBuilder.Build();

        var service = new OllamaAiService(
            kernel,
            new NullLogger<OllamaAiService>());

        var input = new Models.ChatMessage("Hi");

        // Act
        var results = service.StreamingQuery(input);

        var combinedResults = new StringBuilder();
        await foreach (var result in results)
        {
            combinedResults.Append(result.Content);
        }

        // Assert
        combinedResults.ToString().Should().Be("Hello world!");
    }
}
