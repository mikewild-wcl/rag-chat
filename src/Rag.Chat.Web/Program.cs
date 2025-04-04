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
builder.Services.AddOpenApi();

builder.Services
    .AddSingleton<OllamaAiService>()
    .AddSingleton<DummyAiService>()
    .AddSingleton<AiServiceFactory>()
    .AddSingleton<IAiService>(sp =>
    {
        var factory = sp.GetRequiredService<AiServiceFactory>();
        return factory.CreateAiService();
    });

builder.Services
    .AddSingleton<IChatClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
    return new OllamaChatClient(new Uri(options.BaseUri), options.ModelName);
});

var app = builder.Build();

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
        [FromBody] Rag.Chat.Core.Models.ChatMessage message, IAiService aiService) =>
{
    var response = new { response = await aiService.Query(message) };
    return Results.Ok(response);
})
    .RequireRateLimiting(ApiRateLimitPolicy)
    .WithSummary("Post a message.")
    .WithDescription("This endpoint handles chat messages and returns a chat response.")
    .WithTags("Chat");

app.Run();
