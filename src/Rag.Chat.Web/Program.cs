using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Services;
using Rag.Chat.Core.Services.Interfaces;
using System.Threading.RateLimiting;

const string ApiRateLimitPolicy = "api";

var builder = WebApplication.CreateBuilder(args);

var rateLimitOptions = new RateLimitOptions();
builder.Services
    .Configure<ApiOptions>(builder.Configuration.GetSection(nameof(ApiOptions)));

builder.Services
    .AddRateLimiter(_ => _
        .AddFixedWindowLimiter(policyName: ApiRateLimitPolicy, options =>
        {
            options.PermitLimit = rateLimitOptions.PermitLimit;
            options.Window = TimeSpan.FromSeconds(rateLimitOptions.Window);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = rateLimitOptions.QueueLimit;
        }));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IAiService, AiService>();
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

    return new OllamaChatClient(new Uri(options.BaseUri), options.ModelName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// static string GetTicks() => (DateTime.Now.Ticks & 0x11111).ToString("00000");

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/chat", async ([FromBody] Rag.Chat.Core.Models.ChatMessage message, IAiService aiService) =>
{
    var response = new { response = await aiService.Query(message) };
    return Results.Ok(response);
})
    .RequireRateLimiting(ApiRateLimitPolicy); ;

app.Run();
