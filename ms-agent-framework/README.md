# Microsoft Agent Framework - Consensus Builder

This directory contains a C# command-line application built with the **Microsoft Agent Framework** (`Microsoft.Agents.AI.OpenAI`) that uses multiple AI models to build consensus responses through multi-turn conversations.

## Quick Start

### 1. Set Environment Variables

```powershell
$env:ASKLLM_API_ENDPOINT = "https://openrouter.ai/api/v1"
$env:ASKLLM_API_KEY = "your-api-key-here"
```

### 2. Run the Application

```powershell
# Run with default files
.\run-consensus.ps1

# Or run with custom files
.\run-consensus.ps1 -PromptFile "path\to\prompt.txt" -ModelsFile "path\to\models.txt"
```

### 3. Or Run Directly

```powershell
cd ConsensusAgent
dotnet run ..\example-prompt.txt ..\models.txt
```

## Files

- **ConsensusAgent/** - The main C# application
- **example-prompt.txt** - Sample prompt about ASD diagnosis research
- **models.txt** - List of OpenRouter models to use
- **run-consensus.ps1** - Helper script to run the application

## How It Works

The application:
1. Reads a prompt and list of models
2. **Initializes one Agent/Thread pair per model** (following Microsoft Agents SDK best practices)
3. Queries each model over 5 rounds of discussion, reusing the same agents/threads
4. Conducts voting after each round to identify consensus
5. Generates a final Markdown consensus document
6. Creates a conversation log showing the full discussion

### Architecture Highlights
- **One Thread Per Model**: Each model maintains its own conversation state across rounds
- **Shared Context**: Models share context through prompt building (including previous responses)
- **Efficient Resource Usage**: 5 agent instances instead of 25+ (one per model, not per request)
- See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation

## Output

Two files are created:
- `consensus-YYYYMMDD-HHMMSS.md` - Final consensus response
- `conversation-log-YYYYMMDD-HHMMSS.txt` - Full conversation history

## Documentation

See [ConsensusAgent/README.md](ConsensusAgent/README.md) for detailed documentation.

## Models

The application works with any OpenRouter-compatible models. The default `models.txt` includes:
- openai/gpt-5
- anthropic/claude-sonnet-4
- x-ai/grok-3
- deepseek/deepseek-chat-v3.1:free
- google/gemini-2.5-pro

You can edit `models.txt` to use different models.
