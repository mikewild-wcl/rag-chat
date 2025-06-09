namespace Rag.Chat.Core.Services;

using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Services.Interfaces;

public class InMemoryChatHistoryPersistenceService : IChatHistoryPersistenceService
{
    private readonly ConcurrentDictionary<string, ChatHistory> _chatHistories = new();

    public Task Remove(string userId)
    {
        _chatHistories.TryRemove(userId, out _);
        return Task.CompletedTask;
    }

    public Task<ChatHistory?> Retrieve(string userId) =>
        Task.FromResult(_chatHistories.TryGetValue(userId, out var chatHistory) ? chatHistory : null);

    public Task Save(string userId, ChatHistory chatHistory)
    {
        _chatHistories[userId] = chatHistory;
        return Task.CompletedTask;
    }
}