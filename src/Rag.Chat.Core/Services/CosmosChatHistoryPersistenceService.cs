using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;
using System.Diagnostics;

namespace Rag.Chat.Core.Services;

public class CosmosChatHistoryPersistenceService(
    CosmosClient cosmosClient,
    IOptions<CosmosDbOptions> cosmosDbOptions,
    ILogger<CosmosChatHistoryPersistenceService> logger) : IChatHistoryPersistenceService
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly CosmosDbOptions _cosmosDbOptions = cosmosDbOptions.Value;
    private readonly ILogger<CosmosChatHistoryPersistenceService> _logger = logger;

    public async Task Remove(string userId)
    {
        try
        {
            var container = await GetContainer();
            await container.DeleteItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId));
            _logger.LogInformation("Removed chat history for userId {UserId}", userId);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error occurred while removing chat history for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing chat history for user {UserId}", userId);
        }
    }

    public async Task<ChatHistory?> Retrieve(string key)
    {
        try
        {
            var container = await GetContainer();

            //https://github.com/microsoft/semantic-kernel/issues/2443
            //https://github.com/microsoft/semantic-kernel/discussions/6582
            //https://github.com/microsoft/semantic-kernel/discussions/5815
            var response = await container.ReadItemAsync<UserChatHistoryContainer>(key, new PartitionKey(key));

            if (response.Resource is null)
            {
                return null;
            }

            return response.Resource.ChatHistory;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task Save(string userId, ChatHistory chatHistory)
    {
        var container = await GetContainer();

        foreach (var msg in chatHistory)
        {
            Debug.WriteLine($"Message: {msg.Role} - {msg.Content}");
        }

        var userChatHistory = new UserChatHistoryContainer(userId, userId, chatHistory);
        var ser = Newtonsoft.Json.JsonConvert.SerializeObject(chatHistory);
        var ser2 = Newtonsoft.Json.JsonConvert.SerializeObject(userChatHistory);

        var response = await container.UpsertItemAsync(userChatHistory, new PartitionKey(userId));
        _logger.LogInformation("Saved chat history for userId {UserId}. Response status {StatusCode}.", userId, response.StatusCode);
    }

    private async Task<Container> GetContainer()
    {
        var databaseResponse = await _cosmosClient
            .CreateDatabaseIfNotExistsAsync(_cosmosDbOptions.DatabaseName);

        var database = databaseResponse.Database;

        var containerResponse = await database
            .CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = _cosmosDbOptions.ContainerName,
                    PartitionKeyPath = "/userId"
                });

        return containerResponse.Container;
    }
}
