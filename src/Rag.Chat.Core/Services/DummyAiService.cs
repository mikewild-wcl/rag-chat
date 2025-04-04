using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;
using System.Text;

namespace Rag.Chat.Core.Services;

public class DummyAiService(
    ILogger<OllamaAiService> logger) : IAiService
{
    private readonly ILogger<OllamaAiService> _logger = logger;

    public async Task<string> Query(Models.ChatMessage message)
    {
        return "This is a hard-coded response from the AI service.";
    }

    public async Task LoadDocuments()
    {        
    }
}
