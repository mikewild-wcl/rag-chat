using Microsoft.SemanticKernel.ChatCompletion;

namespace Rag.Chat.Core.Models;

public record UserChatHistoryContainer(
    string Id,
    string UserId,
    ChatHistory ChatHistory);
