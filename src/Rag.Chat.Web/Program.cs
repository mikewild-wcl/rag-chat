using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OllamaSharp;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.Services.Interfaces;
using Scalar.AspNetCore;
using System.ComponentModel;
using System.Threading.RateLimiting;

const string ApiRateLimitPolicy = "api";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .Configure<AiServiceOptions>(builder.Configuration.GetSection(nameof(AiServiceOptions)));

var rateLimitOptions = new RateLimitOptions();
builder.Configuration.GetSection(RateLimitOptions.RateLimit).Bind(rateLimitOptions);

builder.Services
    .AddRateLimiter(_ => _
        .AddFixedWindowLimiter(policyName: ApiRateLimitPolicy, options =>
        {
            options.PermitLimit = rateLimitOptions.PermitLimit;
            options.Window = TimeSpan.FromSeconds(rateLimitOptions.Window);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = rateLimitOptions.QueueLimit;
        }));

builder.Services.AddRazorPages();

//TODO: Look into Semantic Kernel here - https://devblogs.microsoft.com/semantic-kernel/introducing-new-ollama-connector-for-local-models/
//builder.AddOllamaApiClient("chat")
//    .AddChatClient()
//    .UseFunctionInvocation()
//    .UseOpenTelemetry(configure: c =>
//        c.EnableSensitiveData = builder.Environment.IsDevelopment());
//builder.AddOllamaApiClient("embeddings")
//    .AddEmbeddingGenerator();


/*********************************************************************/

//https://bartwullems.blogspot.com/2024/10/semantic-kernelgiving-new-ollama.html

//Add keyed Ollama clients
builder.AddKeyedOllamaApiClient("chat");
builder.AddKeyedOllamaApiClient("embeddings");

//builder.Services.AddKeyedSingleton<OllamaApiClient>("chat", (serviceProvider, _) =>
/*
builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, (serviceProvider, _) =>
{
    var client = (OllamaApiClient)serviceProvider.GetKeyedService<IOllamaApiClient>(connectionName);
    return new OllamaTextGenerationService(client.SelectedModel, client);
}
*/
/*
var kernelBuilder = builder.Services.AddKernel();

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernelBuilder
    .AddOllamaChatCompletion(serviceId: "chat")
    .AddOllamaEmbeddingGenerator(serviceId: "embedding");
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Kernel kernel = kernelBuilder
    .Build();
*/

builder.Services.AddKeyedTransient(Constants.OllamaKernelKey, (sp, key) =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    //TODO: Add keys to Constants class

    //Workaround for errors with IOllamaApiClient etc - create as keyed services above
    // and cast here.
    // Inspired by https://github.com/microsoft/semantic-kernel/issues/10532
    var embeddingClient = sp.GetKeyedService<IOllamaApiClient>("embeddings") as OllamaApiClient;
    var chatClient = sp.GetKeyedService<IOllamaApiClient>("chat") as OllamaApiClient;
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    kernelBuilder
        .AddOllamaChatCompletion(ollamaClient: chatClient, serviceId: "chat")
        .AddOllamaEmbeddingGenerator(serviceId: "embedding");
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    return kernelBuilder.Build();
});

builder.Services
    .AddScoped<IDocumentIngestionService, DocumentIngestionService>();

//Can these services be transient?
builder.Services
    .AddTransient<OllamaAiService>()
    .AddTransient<DummyAiService>()
    .AddTransient<AiServiceFactory>()
    .AddSingleton(sp =>
    {
        var factory = sp.GetRequiredService<AiServiceFactory>();
        return factory.CreateAiService();
    });

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/chat",
    async (
        [Description("Chat prompt message.")]
        [FromBody] Rag.Chat.Core.Models.ChatMessage message,
        IAiService aiService) =>
{
    //var response = new { response = await aiService.Query(message) };
    //return Results.Ok(response);
    return Results.Ok(new { response = await aiService.Query(message) });
})
    .RequireRateLimiting(ApiRateLimitPolicy)
    .WithSummary("Post a message.")
    .WithDescription("This endpoint handles chat messages and returns a chat response.")
    .WithTags("Chat");

app.MapPost("/api/chat-stream",
    (
        [Description("Chat prompt message with streamed response.")]
        [FromBody] Rag.Chat.Core.Models.ChatMessage message,
        IAiService aiService) =>
            PostChatPrompt(message, aiService))
    .RequireRateLimiting(ApiRateLimitPolicy)
    .WithSummary("Post a chat message.")
    .WithDescription("This endpoint handles chat messages and returns a streaming chat response.")
    .WithTags("Chat");

app.Run();

static async IAsyncEnumerable<TokenizedResponse> PostChatPrompt(
    Rag.Chat.Core.Models.ChatMessage prompt,
    IAiService aiService)
{
    await foreach (var token in aiService.StreamingQuery(prompt))
    {
        yield return token;
    }
}
