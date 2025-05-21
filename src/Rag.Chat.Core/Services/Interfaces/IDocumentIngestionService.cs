using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.Services.Interfaces;

public interface IDocumentIngestionService
{
    Task LoadDocuments();
}
