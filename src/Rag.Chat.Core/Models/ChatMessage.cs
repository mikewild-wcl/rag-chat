using System.ComponentModel;

namespace Rag.Chat.Core.Models;

public record ChatMessage(
    [Description("Chat prompt message string.")]
    string Message)
{
    public Guid? UserId { get; init; }
}