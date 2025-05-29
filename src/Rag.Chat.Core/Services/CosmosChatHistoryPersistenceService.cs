namespace Rag.Chat.Core.Services;

using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services.Interfaces;

public class CosmosChatHistoryPersistenceService : IChatHistoryPersistenceService
{
    private readonly Container _container;

    public CosmosChatHistoryPersistenceService(CosmosClient cosmosClient, string databaseId, string containerId)
    {
        // https://github.com/Azure/azure-cosmos-dotnet-v3/issues/4900

        _container = cosmosClient.GetContainer(databaseId, containerId);
    }

    public async Task<ChatHistory?> Retrieve(string key)
    {
        try
        {
            var response = await _container.ReadItemAsync<ChatHistory>(key, new PartitionKey(key));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task Save(string key, ChatHistory chatHistory)
    {
        await _container.UpsertItemAsync(chatHistory, new PartitionKey(key));
    }
}