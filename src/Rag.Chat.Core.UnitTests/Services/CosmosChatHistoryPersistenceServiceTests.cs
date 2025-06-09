using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using System.Net;

namespace Rag.Chat.Core.UnitTests.Services;

public class CosmosChatHistoryPersistenceServiceTests
{
    private readonly Mock<CosmosClient> _mockCosmosClient = new();
    private readonly Mock<Container> _mockContainer = new();
    private readonly Mock<ILogger<CosmosChatHistoryPersistenceService>> _mockLogger = new();
    private readonly CosmosDbOptions _cosmosDbOptions = new()
    {
        DatabaseName = "TestDatabase",
        ContainerName = "TestContainer"
    };

    private CosmosChatHistoryPersistenceService CreateService()
    {
        var mockContainerResponse = new Mock<ContainerResponse>();
        mockContainerResponse.Setup(r => r.Container).Returns(_mockContainer.Object);

        var mockDatabase = new Mock<Database>();
        mockDatabase.
            Setup(d => d.Id)
                .Returns(_cosmosDbOptions.DatabaseName);

        var mockDatabaseResponse = new Mock<DatabaseResponse>();
        mockDatabaseResponse.Setup(r => r.Database).Returns(mockDatabase.Object);

        mockDatabase
            .Setup(d => d.CreateContainerIfNotExistsAsync(It.IsAny<ContainerProperties>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockContainerResponse.Object);
        mockDatabase
            .Setup(d => d.CreateContainerIfNotExistsAsync(It.IsAny<ContainerProperties>(), It.IsAny<ThroughputProperties>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockContainerResponse.Object);

        _mockCosmosClient
            .Setup(c => c.CreateDatabaseIfNotExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDatabaseResponse.Object);

        //_mockCosmosClient
        //    .Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(_mockContainer.Object);

        return new CosmosChatHistoryPersistenceService(
            _mockCosmosClient.Object,
            Options.Create(_cosmosDbOptions),
            _mockLogger.Object);
    }

    [Fact]
    public async Task Retrieve_Should_Return_ChatHistory_When_Exists()
    {
        // Arrange
        var service = CreateService();
        var userId = "test-user";
        var chatHistory = new ChatHistory();
        var userChatHistory = new UserChatHistoryContainer(userId, userId, chatHistory);

        var itemResponse = new Mock<ItemResponse<UserChatHistoryContainer>>();
        itemResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);
        itemResponse.Setup(r => r.Resource).Returns(userChatHistory);

        _mockContainer
            .Setup(c => c.ReadItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId), null, default))
            .ReturnsAsync(itemResponse.Object);

        // Act
        var result = await service.Retrieve(userId);

        // Assert
        result.Should().BeEquivalentTo(chatHistory);
    }

    [Fact]
    public async Task Retrieve_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var service = CreateService();
        var userId = "nonexistent-user";

        _mockContainer
            .Setup(c => c.ReadItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await service.Retrieve(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Save_Should_Upsert_ChatHistory()
    {
        // Arrange
        var service = CreateService();
        var userId = "test-user";
        var chatHistory = new ChatHistory();
        var userChatHistory = new UserChatHistoryContainer(userId, userId, chatHistory);

        var itemResponse = new Mock<ItemResponse<UserChatHistoryContainer>>();
        itemResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);
        itemResponse.Setup(r => r.Resource).Returns(userChatHistory);

        _mockContainer
            .Setup(c => c.UpsertItemAsync(userChatHistory, new PartitionKey(userId), null, default))
            .ReturnsAsync(itemResponse.Object);

        // Act
        await service.Save(userId, chatHistory);

        // Assert
        _mockContainer.Verify(c => c.UpsertItemAsync(It.IsAny<UserChatHistoryContainer>(), It.IsAny<PartitionKey>(), null, default), Times.Once);
    }

    [Fact]
    public async Task Remove_Should_Delete_ChatHistory()
    {
        // Arrange
        var service = CreateService();
        var userId = "test-user";

        var itemResponse = new Mock<ItemResponse<UserChatHistoryContainer>>();
        itemResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.NoContent);

        _mockContainer
            .Setup(c => c.DeleteItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId), null, default))
            .ReturnsAsync(itemResponse.Object);

        // Act
        await service.Remove(userId);

        // Assert
        _mockContainer.Verify(c => c.DeleteItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId), null, default), Times.Once);
    }
}