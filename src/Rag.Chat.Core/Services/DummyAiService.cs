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

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(Models.ChatMessage message)
    {
        yield return new TokenizedResponse("This");
        await Task.Delay(500);
        yield return new TokenizedResponse(" is a");
        await Task.Delay(700);
        yield return new TokenizedResponse(" hard-coded");
        await Task.Delay(500);
        yield return new TokenizedResponse(" streaming response");
        await Task.Delay(500);
        yield return new TokenizedResponse(" from");
        await Task.Delay(800);
        yield return new TokenizedResponse(" the AI service.");
    }

    public async Task LoadDocuments()
    {
    }
}
