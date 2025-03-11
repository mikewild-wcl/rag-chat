namespace ChatSample.Configuration;

public record ApiSettings
{
    public required string BaseUri { get; init; }

    public required string ApiKey { get; init; }

    public int Timeout { get; init; }
}