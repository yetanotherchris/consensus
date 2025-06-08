# consensus

A simple console application to ask multiple large language models for a consensus answer using [openrouter.ai](https://openrouter.ai/).

## Building

```
dotnet build
```

Run the application with:

```
dotnet run --project src/ConsensusApp/ConsensusApp -- "your question here"
```

The program will prompt you for models and your OpenRouter API key if not provided in the `OPENROUTER_API_KEY` environment variable.
