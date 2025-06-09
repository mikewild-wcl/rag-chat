namespace Rag.Chat.Core.Models;

public record CosmosDbOptions
{
    public required string DatabaseName { get; init; }

    public required string ContainerName { get; init; }
}