using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var ollamaModelParameter = builder.Configuration[$"parameters:OllamaModel"];
var ollamaEmbeddingModelParameter = builder.Configuration[$"parameters:OllamaEmbeddingModel"];

var ollama = builder.AddOllama("ollama")
    .WithDataVolume();

#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
// Remove "Preview" from the command below to use the stable version of the emulator.
var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });
#pragma warning restore ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var chat = ollama.AddModel("chat", ollamaModelParameter!);
var embeddings = ollama.AddModel("embeddings", ollamaEmbeddingModelParameter!);

var web = builder
    .AddProject<Rag_Chat_Web>("rag-chat-web-app")
    .WithReference(chat)
    .WithReference(embeddings)
    .WaitFor(chat)
    .WaitFor(embeddings);

builder.Build().Run();
