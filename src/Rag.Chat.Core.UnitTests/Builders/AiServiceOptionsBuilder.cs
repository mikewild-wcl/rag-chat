using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.UnitTests.Builders;

public class AiServiceOptionsBuilder
{
    private const string DefaultApiKey = "TEST_API_KEY";
    private const string DefaultBaseUri = "https://test.connect.co.uk/";
    private const string DefaultModelName = "test_model";
    private const string DefaultServiceType = "Ollama";
    private const int DefaultTimeout = 30;

    public static AiServiceOptions Build(
        string apiKey = DefaultApiKey,
        string baseUri = DefaultBaseUri,
        string modelName = DefaultModelName,
        string serviceType = DefaultServiceType,
        int timeout = DefaultTimeout) => new()
        {
            ApiKey = apiKey,
            BaseUri = baseUri,
            ModelName = modelName,
            ServiceType = serviceType,
            Timeout = timeout,
        };
}
