using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.Services.Interfaces;

public interface IAiService
{
    Task<string> Query(ChatMessage message);

    IAsyncEnumerable<TokenizedResponse> StreamingQuery(ChatMessage message);
    
    Task ClearChat(Guid? userId);
}
