using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;
using System.Text;

namespace Rag.Chat.Core.Services;

public class OllamaAiService(
    [FromKeyedServices(Constants.OllamaKernelKey)]
    Kernel kernel,
    ILogger<OllamaAiService> logger) : IAiService
{
    private readonly Kernel _kernel = kernel;
    private readonly ILogger<OllamaAiService> _logger = logger;

    //private List<Microsoft.Extensions.AI.ChatMessage> _chatHistory = [];
    private ChatHistory _chatHistory = [];

    public async Task<string> Query(Models.ChatMessage message)
    {
        var responses = new StringBuilder();

        // TODO: chatHistory should be keyed by user or session and cached
        //_chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, message.Message));
        _chatHistory.AddUserMessage(message.Message);

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();        
        //await foreach (var item in chatClient.GetStreamingResponseAsync(_chatHistory))
        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {            
            if (item.Metadata?.Any() == true)
            {
                foreach (var property in item.Metadata)
                {
                    _logger.LogInformation("AI response has additional property {Key} = {Value}", property.Key, property.Value);
                }
            }

            _logger.LogInformation("AI response '{Content}'", item.Content);
            responses.Append(item.Content);
        }

        //_chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, responses.ToString()));

        return responses.ToString();
    }

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(Models.ChatMessage message)
    {
        //_chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, message.Message));
        _chatHistory.AddUserMessage(message.Message);

        //var services = _kernel.GetAllServices<IChatCompletionService>();
        //foreach (var item in services)
        //{

        //}

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        //await foreach (var item in chatClient.GetStreamingResponseAsync(_chatHistory))
        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            if (string.IsNullOrEmpty(item.Content))
            {
                continue;
            }

            await Task.Delay(300); //Delay so we only send one token at a time

            yield return new TokenizedResponse(item.Content);
        }
    }    
}