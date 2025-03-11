using ChatSample.Models;

namespace ChatSample.Services;

public interface IAiService
{
    Task LoadDocuments();

    Task<string> Query(ChatMessage message);
}
