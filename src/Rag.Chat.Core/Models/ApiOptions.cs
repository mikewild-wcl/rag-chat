namespace Rag.Chat.Core.Models;

public record ApiOptions
{
    public required string BaseUri { get; init; }

    public required string ApiKey { get; init; }

    public required string ModelName { get; init; }

    public int Timeout { get; init; }
}