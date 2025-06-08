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

You can optionally create a markdown log file of each model's response. When prompted, choose `Minimal` to log short summaries of each model's changes or `Full` to store every response separated by `-----------` lines. At the end of the log file each model's summary from within `<ConsensusSummary>` tags is also collected for quick reference.
By default the log and response files use the first ten characters of the prompt for their names and are overwritten on each run. Set the `CONSENSUS_UNIQUE_FILES` environment variable to create new files with a timestamp instead.
The system prompts used for each model are stored as text files in `Resources` and embedded in the application.
