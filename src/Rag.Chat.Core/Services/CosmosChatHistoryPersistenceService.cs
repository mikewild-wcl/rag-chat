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

    public async Task<ChatHistory?> Retrieve(string userId)
    {
        try
        {
            var container = await GetContainer();

            //https://github.com/microsoft/semantic-kernel/issues/2443
            //https://github.com/microsoft/semantic-kernel/discussions/6582
            //https://github.com/microsoft/semantic-kernel/discussions/5815

            //https://stackoverflow.com/questions/76219161/cosmosclient-custom-json-converter-works-out-of-the-box-with-newtonsoft-not-w
            //https://www.billtalkstoomuch.com/2023/03/14/cosmosdb-system-text-json-i-take-it-all-back/amp/
            //Custom serializer - https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/SystemTextJson/CosmosSystemTextJsonSerializer.cs?ref=billtalkstoomuch.com

            /*
              The JSON payload for polymorphic interface or abstract type 'Microsoft.SemanticKernel.KernelContent' must specify a type discriminator. Path: $.chatHistory[0].items[0] | LineNumber: 0 | BytePositionInLine: 185.
              https://stackoverflow.com/questions/77669675/system-text-json-polymorphic-deserialization-exception-when-type-is-not-the-fir
              https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism
                [JsonDerivedType(typeof(WeatherForecastWithCity))]
             */
            var response = await container.ReadItemAsync<UserChatHistoryContainer>(userId, new PartitionKey(userId));

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving chat history for key {Key}", userId);
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
