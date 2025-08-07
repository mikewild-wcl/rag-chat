namespace Rag.Chat.Core.Models;

public record AiServiceOptions
{
    public required string BaseUri { get; init; }

    public required string ApiKey { get; init; }

    public required string ModelName { get; init; }

    public required string ServiceType { get; init; }

    public required int MessageDelayInMilliseconds { get; init; }

    public int Timeout { get; init; }
}