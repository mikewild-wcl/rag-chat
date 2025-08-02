using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.UnitTests.Builders;

public class AiServiceBuilder
{
    private const string DefaultApiKey = "TEST_API_KEY";
    private const string DefaultBaseUri = "https://test.connect.co.uk/";
    private const string DefaultModelName = "test_model";
    private const string DefaultServiceType = "Ollama";
    private const int DefaultTimeout = 30;

    public static OllamaAiService Build(
        Kernel? kernel = null,
        IChatHistoryPersistenceService? chatHistoryPersistenceService = null,
        ILogger<OllamaAiService>? logger = null) =>
        new(kernel ?? new Kernel(),
            chatHistoryPersistenceService ?? Mock.Of<IChatHistoryPersistenceService>(),
            logger ?? new NullLogger<OllamaAiService>()            );
}
