using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.Services.Interfaces;

public interface IAiService
{
    Task LoadDocuments();

    Task<string> Query(ChatMessage message);
}
