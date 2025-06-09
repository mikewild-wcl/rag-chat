namespace Rag.Chat.Core.UnitTests.Services;

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services;
using Xunit;

public class InMemoryChatHistoryPersistenceServiceTests
{
    [Fact]
    public async Task Retrieve_Should_Return_Null_For_NonExistent_Key()
    {
        // Arrange
        var service = new InMemoryChatHistoryPersistenceService();

        // Act
        var result = await service.Retrieve("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Save_Should_Store_And_Retrieve_ChatHistory()
    {
        // Arrange
        var service = new InMemoryChatHistoryPersistenceService();
        var chatHistory = new ChatHistory();

        // Act
        await service.Save("test-key", chatHistory);
        var result = await service.Retrieve("test-key");

        // Assert
        result.Should().BeSameAs(chatHistory);
    }

    [Fact]
    public async Task Save_Should_Overwrite_Existing_ChatHistory()
    {
        // Arrange
        var service = new InMemoryChatHistoryPersistenceService();
        var initialChatHistory = new ChatHistory();
        var updatedChatHistory = new ChatHistory();

        // Act
        await service.Save("test-key", initialChatHistory);
        await service.Save("test-key", updatedChatHistory);
        var result = await service.Retrieve("test-key");

        // Assert
        result.Should().BeSameAs(updatedChatHistory);
    }

    [Fact]
    public async Task Remove_Should_Delete_ChatHistory()
    {
        // Arrange
        var service = new InMemoryChatHistoryPersistenceService();
        var chatHistory = new ChatHistory();
        var userId = "test-user";

        await service.Save(userId, chatHistory);

        // Act
        await service.Remove(userId);
        var result = await service.Retrieve(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Remove_Should_Not_Throw_Exception_When_Key_Does_Not_Exist()
    {
        // Arrange
        var service = new InMemoryChatHistoryPersistenceService();
        var chatHistory = new ChatHistory();
        var userId = "test-user";

        // Act
        await service.Remove(userId);
        var result = await service.Retrieve(userId);

        // Assert
        result.Should().BeNull();
    }
}