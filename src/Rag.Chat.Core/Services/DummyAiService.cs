using Microsoft.Extensions.Logging;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.Services;

public class DummyAiService(
    ILogger<DummyAiService> logger) : IAiService
{
    private readonly ILogger<DummyAiService> _logger = logger;

    public int DelayBetweenMessages { get; set; } = 300;

    public Task ClearChat(Guid? userId)
    {
        // No operation for the dummy service
        return Task.CompletedTask;
    }

    public Task<string> Query(ChatMessage message)
    {
        _logger.LogInformation("{Service} {Method} called.", nameof(DummyAiService), nameof(StreamingQuery));
        return Task.FromResult("This is a hard-coded response from the AI service.");
    }

    public async IAsyncEnumerable<TokenizedResponse> StreamingQuery(ChatMessage message)
    {
        _logger.LogInformation("{Service} {Method} called and will start yielding results. Delay is {Delay}",
                nameof(DummyAiService),
                nameof(StreamingQuery),
                DelayBetweenMessages);

        yield return new TokenizedResponse("This");
        await Task.Delay(DelayBetweenMessages);
        yield return new TokenizedResponse(" is a");
        await Task.Delay(DelayBetweenMessages);
        yield return new TokenizedResponse(" hard-coded");
        await Task.Delay(DelayBetweenMessages);
        yield return new TokenizedResponse(" streaming response");
        await Task.Delay(DelayBetweenMessages);
        yield return new TokenizedResponse(" from");
        await Task.Delay(DelayBetweenMessages);
        yield return new TokenizedResponse(" the AI service.\n");
    }
}
