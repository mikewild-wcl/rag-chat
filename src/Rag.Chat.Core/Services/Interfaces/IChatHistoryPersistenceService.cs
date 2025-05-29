using Microsoft.SemanticKernel.ChatCompletion;

namespace Rag.Chat.Core.Services.Interfaces;

public interface IChatHistoryPersistenceService
{
    Task<ChatHistory?> Retrieve(string key);

    Task Save(string key, ChatHistory chatHistory);
}
