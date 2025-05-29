using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;
using System.Text;

namespace Rag.Chat.Core.Services;

public class OllamaAiService(
    [FromKeyedServices(Constants.OllamaKernelKey)]
    Kernel kernel,
    IChatHistoryPersistenceService chatHistoryPersistenceService,
    ILogger<OllamaAiService> logger) : IAiService
{
    private readonly Kernel _kernel = kernel;
    private readonly IChatHistoryPersistenceService _chatHistoryPersistenceService = chatHistoryPersistenceService;
    private readonly ILogger<OllamaAiService> _logger = logger;

    public async Task<string> Query(ChatMessage message)
    {
        var responses = new StringBuilder();

        await foreach (var item in StreamingQuery(message))
        {
            responses.Append(item.Content);
        }

        return responses.ToString();
    }

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(ChatMessage message)
    {
        //if(message?.Message is null)
        //{
        //    yield return default;
        //}

        var chatHistoryKey = message?.UserId?.ToString();
        var chatHistory = ((chatHistoryKey is not null) 
            ? await _chatHistoryPersistenceService.Retrieve(chatHistoryKey)
            : null)
            ?? [];

        chatHistory.AddUserMessage(message.Message);

        var responses = new StringBuilder();

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            if (string.IsNullOrEmpty(item.Content))
            {
                continue;
            }

            //await Task.Delay(300); //Delay so we only send one token at a time
            responses.Append(item.Content);
            yield return new TokenizedResponse(item.Content);
        }

        //TODO: Collect response and add to chat history
        //https://github.com/microsoft/semantic-kernel/discussions/8105
        chatHistory.AddAssistantMessage(responses.ToString());

        if (chatHistoryKey is not null)
        {
            await _chatHistoryPersistenceService.Save(chatHistoryKey, chatHistory);
        }
    }
}