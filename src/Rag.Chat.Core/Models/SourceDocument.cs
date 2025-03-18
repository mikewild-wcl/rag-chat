namespace Rag.Chat.Core.Models;

public record SourceDocument(
    string SourceUri, 
    string Title, 
    string Type)
{
}
