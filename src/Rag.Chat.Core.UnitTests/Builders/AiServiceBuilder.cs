using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.UnitTests.Builders;

public static class AiServiceBuilder
{
    public static OllamaAiService Build(
        Kernel? kernel = null,
        IChatHistoryPersistenceService? chatHistoryPersistenceService = null,
        ILogger<OllamaAiService>? logger = null) =>
        new(kernel ?? new Kernel(),
            chatHistoryPersistenceService ?? Mock.Of<IChatHistoryPersistenceService>(),
            logger ?? new NullLogger<OllamaAiService>());

    public static DummyAiService BuildDummyService(
        IOptions<AiServiceOptions>? options = null,
        ILogger<DummyAiService> ? logger = null) =>
        new(options ?? Options.Create(AiServiceOptionsBuilder.Build()),
            logger ?? new NullLogger<DummyAiService>());
}
