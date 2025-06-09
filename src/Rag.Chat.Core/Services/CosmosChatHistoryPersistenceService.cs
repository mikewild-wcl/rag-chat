using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;

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
            var response = await container.ReadItemAsync<UserChatHistoryContainer>(key, new PartitionKey(key));

            if(response.Resource is null)
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

        var userChatHistory = new UserChatHistoryContainer(userId, userId, chatHistory);

        //https://stackoverflow.com/questions/69070451/getting-one-of-the-specified-inputs-is-invalid-in-azure-cosmosdb-patchitemasyn
        var response = await container.UpsertItemAsync(userChatHistory, new PartitionKey(userId));
        _logger.LogInformation("Saved chat history for userId {UserId}. Response status {StatusCode}.", userId, response.StatusCode);
    }

    private async Task<Container> GetContainer()
    {
        var database = await _cosmosClient
            .CreateDatabaseIfNotExistsAsync(_cosmosDbOptions.DatabaseName);
        var container = await database
            .Database
            .CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = _cosmosDbOptions.ContainerName,
                    PartitionKeyPath = "/userId"
                });

        return container;
    }
}
