using ChatSample.Configuration;
using ChatSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rag.Chat.Web.Models;

namespace ChatSample.Services;

public class AiService(
    IConfiguration configuration,
    IOptions<ApiSettings> apiOptions) : IAiService
{
    private ApiSettings _apiSettings = apiOptions.Value;

    public async Task<string> Query(ChatMessage message)
    {
        var response = $"You said: {message.Message}";
        return response;
    }

    public async Task LoadDocuments()
    {
        var sources = configuration.GetSection(nameof(SourceDocuments)).Get<List<SourceDocument>>();

        if (sources is not null)
        {
            foreach (var source in sources)
            {
                Console.WriteLine($"Found document source: {source.SourceUri}");
            }
        }
    }
}