using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services.Interfaces;
using Rag.Chat.Core.UnitTests.Builders;
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
        var act = () => AiServiceBuilder.Build(kernel);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ClearChat_Should_Clear_Conversation()
    {
        // Arrange
        var mockChatHistoryPersistenceService = new Mock<IChatHistoryPersistenceService>();
        var kernel = Kernel.CreateBuilder().Build();
        var service = AiServiceBuilder.Build(
            kernel,
            mockChatHistoryPersistenceService.Object);

        var userId = Guid.NewGuid();

        // Act
        await service.ClearChat(userId);

        // Assert
        mockChatHistoryPersistenceService.Verify(
            x => x.Remove(userId.ToString()),
            Times.Once);
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

        var service = AiServiceBuilder.Build(kernel);

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

        var service = AiServiceBuilder.Build(kernel);

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

    [Fact]
    public async Task StreamingQuery_Should_Persist_Chat_History()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ChatHistory chatHistory = [new ChatMessageContent { Role = AuthorRole.Developer, Content = "test" }];

        var mockChatHistoryPersistenceService = new Mock<IChatHistoryPersistenceService>();
        mockChatHistoryPersistenceService
            .Setup(x => x.Retrieve(userId.ToString()))
            .ReturnsAsync(chatHistory);

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
            }
            .ToAsyncEnumerable());

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(mockChatCompletion.Object);
        var kernel = kernelBuilder.Build();

        var service = AiServiceBuilder.Build(
            kernel,
            mockChatHistoryPersistenceService.Object);

        var input = new Models.ChatMessage("Hi")
        {
            UserId = userId
        };

        // Act
        await foreach (var _ in service.StreamingQuery(input)) ; //loop and discard results

        // Assert
        mockChatHistoryPersistenceService.Verify(
            x => x.Retrieve(userId.ToString()),
            Times.Once);
        mockChatHistoryPersistenceService.Verify(
            x => x.Save(
                userId.ToString(),
                It.Is<ChatHistory>(x => x != null)),
            Times.Once);
    }

}
