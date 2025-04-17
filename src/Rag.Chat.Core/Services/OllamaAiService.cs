using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;
using System.Text;

namespace Rag.Chat.Core.Services;

public class OllamaAiService(
    IChatClient chatClient,
    ILogger<OllamaAiService> logger,
    IConfiguration configuration,
    IOptions<AiServiceOptions> aiServiceOptions) : IAiService
{
    private readonly AiServiceOptions _aiServiceOptions = aiServiceOptions.Value;
    private readonly ILogger<OllamaAiService> _logger = logger;

    private List<Microsoft.Extensions.AI.ChatMessage> _chatHistory = new();

    public async Task<string> Query(Models.ChatMessage message)
    {
        var responses = new StringBuilder();

        // TODO: chatHistory should be keyed by user or session and cached
        _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, message.Message));

        await foreach (var item in chatClient.GetStreamingResponseAsync(_chatHistory))
        {
            //_logger.LogInformation("AI response is by author {Author}", item.AuthorName);
            //_logger.LogInformation("AI response has role {Role}", item.Role);
            //_logger.LogInformation("AI response finish reason {FinishReason}", item.FinishReason);

            if (item.AdditionalProperties?.Any() == true)
            {
                foreach (var property in item.AdditionalProperties)
                {
                    _logger.LogInformation("AI response has additional property {Key} = {Value}", property.Key, property.Value);
                }
            }

            _logger.LogInformation("AI response '{Text}'", item.Text);
            responses.Append(item.Text);
        }

        _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, responses.ToString()));

        return responses.ToString();
    }

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(Models.ChatMessage message)
    {
        _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, message.Message));

        await foreach (var item in chatClient.GetStreamingResponseAsync(_chatHistory))
        {
            yield return new TokenizedResponse(item.Text);
        }
    }

    public async Task LoadDocuments()
    {
        var sources = configuration
            .GetSection(nameof(SourceDocuments))
            .Get<List<SourceDocument>>();

        if (sources is not null)
        {
            foreach (var source in sources)
            {
                // Console.WriteLine($"Found document source: {source.SourceUri}");
                _logger.LogInformation("Found document source: {SourceUri}", source.SourceUri);
            }
        }
    }
}