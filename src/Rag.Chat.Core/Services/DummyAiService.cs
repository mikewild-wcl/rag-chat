using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.Services;

public class DummyAiService(
    IOptions<AiServiceOptions> options,
    ILogger<DummyAiService> logger) : IAiService
{
    private readonly AiServiceOptions _options = options.Value;
    private readonly ILogger<DummyAiService> _logger = logger;

    public Task ClearChat(string? userId)
    {        
        return Task.CompletedTask; // No operation for the dummy service
    }

    public Task<string> Query(ChatMessage message)
    {
        _logger.LogInformation("{Service} {Method} called.", nameof(DummyAiService), nameof(StreamingQuery));
        return Task.FromResult("This is a hard-coded response from the AI service.");
    }

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(ChatMessage message)
    {
    var delayBetweenMessages = _options.MessageDelayInMilliseconds;

    _logger.LogInformation("{Service} {Method} called and will start yielding results. Delay between messages is {Delay}",
                nameof(DummyAiService),
                nameof(StreamingQuery),
                delayBetweenMessages);

        yield return new TokenizedResponse("This");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" is a");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" hard-coded");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" streaming response");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" from");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" the AI service.\n");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse("\n");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" And here is");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" another");
        await Task.Delay(delayBetweenMessages);
        yield return new TokenizedResponse(" paragraph.\n");
    }
}
