namespace Rag.Chat.Core.Services;

using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services.Interfaces;

public class InMemoryChatHistoryPersistenceService : IChatHistoryPersistenceService
{
    private readonly ConcurrentDictionary<string, ChatHistory> _chatHistories = new();

    public Task<ChatHistory?> Retrieve(string key) =>
        Task.FromResult(_chatHistories.TryGetValue(key, out var chatHistory) ? chatHistory : null);

    public Task Save(string key, ChatHistory chatHistory)
    {
        _chatHistories[key] = chatHistory;
        return Task.CompletedTask;
    }
}