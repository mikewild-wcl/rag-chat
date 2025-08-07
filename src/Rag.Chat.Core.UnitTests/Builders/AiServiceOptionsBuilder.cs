using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.UnitTests.Builders;

public static class AiServiceOptionsBuilder
{
    private const string DefaultApiKey = "TEST_API_KEY";
    private const string DefaultBaseUri = "https://test.connect.co.uk/";
    private const string DefaultModelName = "test_model";
    private const string DefaultServiceType = "Ollama";
    private const int DefaultMessageDelayInMilliseconds = 0;
    private const int DefaultTimeout = 30;

    public static AiServiceOptions Build(
        string apiKey = DefaultApiKey,
        string baseUri = DefaultBaseUri,
        string modelName = DefaultModelName,
        string serviceType = DefaultServiceType,
        int messageDelayInMilliseconds = DefaultMessageDelayInMilliseconds,
        int timeout = DefaultTimeout) => new()
        {
            ApiKey = apiKey,
            BaseUri = baseUri,
            ModelName = modelName,
            ServiceType = serviceType,
            MessageDelayInMilliseconds = messageDelayInMilliseconds,
            Timeout = timeout,
        };
}
