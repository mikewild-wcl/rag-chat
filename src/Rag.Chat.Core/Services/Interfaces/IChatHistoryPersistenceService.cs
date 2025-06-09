using Microsoft.SemanticKernel.ChatCompletion;

namespace Rag.Chat.Core.Services.Interfaces;

public interface IChatHistoryPersistenceService
{
    Task Remove(string userId);

    Task<ChatHistory?> Retrieve(string userId);

    Task Save(string userId, ChatHistory chatHistory);
}
