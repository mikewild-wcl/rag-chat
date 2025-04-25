using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var ollamaModelParameter = builder.Configuration[$"parameters:OllamaModel"];
var ollamaEmbeddingModelParameter = builder.Configuration[$"parameters:OllamaEmbeddingModel"];

var ollama = builder.AddOllama("ollama")
    .WithDataVolume();

var chat = ollama.AddModel("chat", ollamaModelParameter!);
var embeddings = ollama.AddModel("embeddings", ollamaEmbeddingModelParameter!);

var web = builder
    .AddProject<Rag_Chat_Web>("rag-chat-web-app")
    .WithReference(chat)
    .WithReference(embeddings)
    .WaitFor(chat)
    .WaitFor(embeddings);

builder.Build().Run();
