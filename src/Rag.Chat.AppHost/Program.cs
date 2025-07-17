using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var ollamaModelParameter = builder.Configuration[$"parameters:OllamaModel"];
var ollamaEmbeddingModelParameter = builder.Configuration[$"parameters:OllamaEmbeddingModel"];

var ollama = builder.AddOllama("ollama")
    .WithDataVolume();

var cosmosConnectionString = builder.Configuration.GetConnectionString("cosmos-db")!;
var cosmosConnection = null as IResourceBuilder<IResourceWithConnectionString>;
var cosmosDb = null as IResourceBuilder<AzureCosmosDBResource>;

if (string.IsNullOrEmpty(cosmosConnectionString))
{
    //https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/199
#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    // Remove "Preview" from the command below to use the stable version of the emulator.
    cosmosDb = builder.AddAzureCosmosDB("cosmos-db")
        .RunAsPreviewEmulator(emulator =>
        {
            emulator.WithDataVolume();
            emulator.WithLifetime(ContainerLifetime.Persistent);
            //emulator.WithDataExplorer();
            //emulator.WithHealthCheck();
            /*
            //https://github.com/dotnet/aspire/issues/5163
            emulator
                .WithHttpEndpoint(51234, 1234, "explorer-port")
                //.WithImageRegistry("mcr.microsoft.com")
                //.WithImage("cosmosdb/linux/azure-cosmos-emulator")
                //.WithImageTag("vnext-preview")
                .WithArgs("--explorer-protocol", "http")
                .WithDataVolume()
                .WithLifetime(ContainerLifetime.Persistent);
            */
        });
    //#pragma warning restore ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
else
{
    cosmosConnection = builder.AddConnectionString("cosmos-db");

    //https://goforgoldman.com/posts/cosmos-aspire-workaround/
    cosmosDb = builder.AddAzureCosmosDB("cosmos-db")
        .WithHttpEndpoint(51234, 1234, "explorer-port") // Enable the Explorer on a custom port
        .WithExternalHttpEndpoints()                   // Expose the ports externally
                                                       // TECH DEBT: Workaround for Explorer dashboard not working in emulator. See: https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/135.
        .RunAsEmulator(cfgContainer =>
        {
            cfgContainer
            .WithImageRegistry("mcr.microsoft.com")        // Set the registry
            .WithImage("cosmosdb/linux/azure-cosmos-emulator") // Use the emulator image
                                                               //.WithImageTag("vnext-preview") // Use the preview tag with the fix
            ;
        });
}

var chat = ollama.AddModel("chat", ollamaModelParameter!);
var embeddings = ollama.AddModel("embeddings", ollamaEmbeddingModelParameter!);

var web = builder
    .AddProject<Rag_Chat_Web>("rag-chat-web-app")
    .WithReference(chat)
    .WithReference(embeddings)
    .WithReference(cosmosConnection is not null 
        ? cosmosConnection
        : cosmosDb!);

web
    .WaitFor(chat)
    .WaitFor(embeddings);

await builder.Build().RunAsync();
