using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services.Interfaces;

namespace Rag.Chat.Core.Services;

public class DocumentIngestionService(
    IConfiguration configuration,
    ILogger<DocumentIngestionService> logger) : IDocumentIngestionService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DocumentIngestionService> _logger = logger;

    public async Task LoadDocuments()
    {
        _logger.LogInformation("{Service} {Method} called.", nameof(DocumentIngestionService), nameof(LoadDocuments));

        var sources = _configuration
            .GetSection(nameof(SourceDocuments))
            .Get<List<SourceDocument>>();

        if (sources is not null)
        {
            foreach (var source in sources)
            {
                // Console.WriteLine($"Found document source: {source.SourceUri}");
                _logger.LogInformation("Found document source: {SourceUri}", source.SourceUri);
            }
        }
    }
}
