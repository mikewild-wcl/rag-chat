# RAG Sample


## Copilot
	- instructions see https://copilot-instructions.md/
	- https://code.visualstudio.com/docs/copilot/copilot-customization
	- 
	- 

## Rate limiting

	See https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0


## CORS
	See https://code-maze.com/enabling-cors-in-asp-net-core/


## Ollama

To run this with Ollama as the LLM back end,

Make sure Ollama is running:
```
ollama serve
```

Download the model if you haven't already
```
ollama pull phi3:mini
```

Run the model
```
ollama run phi3:mini
```

To exit, just type `/bye` in the prompt.


## Streamed response

Using code inspired by https://davidpine.net/blog/dotnet-async-enumerable/ to stream the results to the client,
and https://github.com/IMJONEZZ/LLMs-in-Production/blob/main/chapters/chapter_8/listing_8.1.extremely_basic_streaming_chat.html for the client code.

**TODO:** look at queueing chat requests as done in the above streaming example.


## Aspire

.NET Aspire was added to this solution. See [Adding .NET Aspire to your existing .NET apps](https://devblogs.microsoft.com/dotnet/adding-dotnet-aspire-to-your-existing-dotnet-apps/)

Ollama is working via Aspire - see
 - [.NET Aspire Community Toolkit Ollama integration](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ollama?tabs=dotnet-cli%2Cdocker)
 - [Enhancing Aspire with AI: integrating Ollama for local error resolution](https://raygun.com/blog/enhancing-aspire-with-ai-with-ollama/)


## OpenApi

OpenApi has been implemented for the web application and enabled when running in development mode.
To see the OpenApi specification browse to https://localhost:7185/openapi/v1.json

Scalar has also been included, and can be used to test the API at https://localhost:7185/scalar/v1. 

See https://devblogs.microsoft.com/dotnet/dotnet9-openapi/ for details on the how OpenApi has been added, and https://www.roundthecode.com/dotnet-blog/swagger-dropped-dotnet-9-what-are-alternatives for some information on OpenApi and Scalar.


## Semantic Kernel

Unit testing - https://devblogs.microsoft.com/semantic-kernel/unit-testing-with-semantic-kernel/

## Links

- Interesting article on authenticated vs unauthenticated chatbots - https://www.linkedin.com/pulse/understanding-authenticated-unauthenticated-apis-using-microsoft-rajendra/

