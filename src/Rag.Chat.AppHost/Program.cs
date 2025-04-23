using Projects;
using YamlDotNet.Core.Tokens;

var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Rag_Chat_Web>("rag-chat-web-app");

builder.Build().Run();
