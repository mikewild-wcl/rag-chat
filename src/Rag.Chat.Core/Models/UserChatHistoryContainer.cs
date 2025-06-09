using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;

namespace Rag.Chat.Core.Models;

public record UserChatHistoryContainer(
    string Id,
    string UserId,
    ChatHistory ChatHistory);
