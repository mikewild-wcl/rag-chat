# RAG Sample


## Copilot
	- instructions see https://copilot-instructions.md/
	- https://code.visualstudio.com/docs/copilot/copilot-customization
	- 
	- 

## Rate limiting

	See https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0

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


## OpenApi

OpenApi has been implemented for the web application and enabled when running in development mode.
To see the OpenApi specification browse to https://localhost:7185/openapi/v1.json

Scalar has also been included, and can be used to test the API at https://localhost:7185/scalar/v1. 

See https://devblogs.microsoft.com/dotnet/dotnet9-openapi/ for details on the how OpenApi has been added, and https://www.roundthecode.com/dotnet-blog/swagger-dropped-dotnet-9-what-are-alternatives for some information on OpenApi and Scalar.



