using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
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

//builder.Services.AddOpenApi();

builder.AddOllamaApiClient("chat")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator();

builder.Services
    .AddSingleton<OllamaAiService>()
    .AddSingleton<DummyAiService>()
    .AddSingleton<AiServiceFactory>()
    .AddSingleton<IAiService>(sp =>
    {
        var factory = sp.GetRequiredService<AiServiceFactory>();
        return factory.CreateAiService();
    });

//builder.Services
//    .AddSingleton<IChatClient>(sp =>
//{
//    var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
//    return new OllamaChatClient(new Uri(options.BaseUri), options.ModelName);
//});

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
        //Results.Ok(new { response = PostChatPromptAndGetString(message, aiService) }))
        PostChatPrompt(message, aiService))
    .RequireRateLimiting(ApiRateLimitPolicy)
    .WithSummary("Post a chat message.")
    .WithDescription("This endpoint handles chat messages and returns a streaming chat response.")
    .WithTags("Chat");

app.MapPost("/api/chat-stream-2",
    (
        [Description("Chat prompt message with streamed response.")]
        [FromBody] Rag.Chat.Core.Models.ChatMessage message, IAiService aiService) =>
    {
        return PostChatPrompt(message, aiService);
    })
    .RequireRateLimiting(ApiRateLimitPolicy)
    .WithSummary("Post a message.")
    .WithDescription("This endpoint handles chat messages and returns a streaming chat response.")
    .WithTags("Chat");

app.Run();

static async IAsyncEnumerable<TokenizedResponse> PostChatPrompt(
    Rag.Chat.Core.Models.ChatMessage prompt,
    IAiService aiService)
{
    //TODO: Should be able to just return await StreamingQuery 
    await foreach (var token in aiService.StreamingQuery(prompt))
    {
        yield return token;
        //yield return new { response = text };
        //yield return resultText;
    }
}

static async IAsyncEnumerable<string> PostChatPromptAndGetString(
    Rag.Chat.Core.Models.ChatMessage prompt,
    IAiService aiService)
{
    //TODO: Should be able to just return await StreamingQuery 
    await foreach (var token in aiService.StreamingQuery(prompt))
    {
        yield return token.Content;
        //yield return new { response = text };
        //yield return resultText;
    }
}
