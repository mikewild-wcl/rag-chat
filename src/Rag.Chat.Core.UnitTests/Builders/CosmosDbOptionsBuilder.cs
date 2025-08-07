using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.UnitTests.Builders;

public static class CosmosDbOptionsBuilder
{
    private const string DefaultDatabaseName = "CosmosTestDatabase";
    private const string DefaultContainerName = "CosmosContainer";

    public static CosmosDbOptions Build(
        string databaseName = DefaultDatabaseName,
        string containerName = DefaultContainerName) => new()
        {
            DatabaseName = databaseName,
            ContainerName = containerName
        };
}
