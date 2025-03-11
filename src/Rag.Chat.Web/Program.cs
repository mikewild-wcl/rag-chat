using ChatSample.Configuration;
using ChatSample.Models;
using ChatSample.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IAiService, AiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapPost("/api/chat", async ([FromBody] ChatMessage message, IAiService aiService) =>
{
    var response = new { response = await aiService.Query(message) };
    return Results.Ok(response);
});

app.Run();
