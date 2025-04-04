using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.Services;

public class AiServiceFactory(
    IServiceProvider serviceProvider, 
    IOptions<AiServiceOptions> aiServiceOptions)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly AiServiceOptions _aiServiceOptions = aiServiceOptions.Value;

    public IAiService CreateAiService()
    {
        return _aiServiceOptions.ServiceType switch
        {
            "Ollama" => _serviceProvider.GetRequiredService<OllamaAiService>(),
            "Dummy" => _serviceProvider.GetRequiredService<DummyAiService>(),
            _ => throw new InvalidOperationException($"Unsupported AI service type: {_aiServiceOptions.ServiceType}")
        };
    }
}
