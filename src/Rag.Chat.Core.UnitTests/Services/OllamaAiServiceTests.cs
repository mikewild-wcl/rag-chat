using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services;

namespace Rag.Chat.Core.UnitTests.Services;

/*
 * Unit testing - https://devblogs.microsoft.com/semantic-kernel/unit-testing-with-semantic-kernel/
*/
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
        var mockConfiguration = new Mock<IConfiguration>();

        var kernel = new Kernel();

        var chatMessages = new List<Microsoft.Extensions.AI.ChatResponseUpdate>
        {
            new(ChatRole.Assistant, "Hello"),
            new(ChatRole.Assistant, " world!")
        };

        //mockKernel
        //    .Setup(c => c.GetStreamingResponseAsync(
        //        It.IsAny<IReadOnlyList<Microsoft.Extensions.AI.ChatMessage>>(),
        //        It.IsAny<ChatOptions?>(),
        //        It.IsAny<CancellationToken>()))
        //    .Returns(TestHelpers.MockAsyncEnumerable<ChatResponseUpdate>(chatMessages));

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
    public async Task DoWorkWithPrompt()
    {
        // Arrange 
        var mockChatCompletion = new Mock<IChatCompletionService>();
        mockChatCompletion
            .Setup(x => x.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ChatMessageContent(AuthorRole.Assistant, "AI response")]);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(mockChatCompletion.Object);

        var kernel = kernelBuilder.Build();
        var service = new OllamaAiService(kernel, new NullLogger<OllamaAiService>());

        // Act 


        // Act
        var result = await service.Query(new Models.ChatMessage("Prompt to AI"));

        // Assert 
        Assert.Equal("AI response", result.ToString());
    }
}
