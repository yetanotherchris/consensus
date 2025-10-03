# Consensus Agent

A C# command-line application that uses multiple AI models to build consensus responses through multi-turn conversations. The application leverages the OpenAI SDK to communicate with various language models via OpenRouter.

## Features

- **Multi-Model Consensus**: Queries multiple LLMs (e.g., GPT-5, Claude, Grok, DeepSeek, Gemini) to build consensus
- **5-Round Discussions**: Models participate in 5 rounds of conversation, building on each other's responses
- **Voting System**: After each round, models vote to identify common themes and disagreements
- **Dual Output**: Generates both a final consensus in Markdown and a detailed conversation log
- **OpenRouter Compatible**: Works with any OpenRouter-compatible model

## Prerequisites

- .NET 9.0 SDK or later
- OpenRouter API access (or compatible endpoint)
- Environment variables configured:
  - `ASKLLM_API_ENDPOINT` - Your LLM API endpoint (e.g., https://openrouter.ai/api/v1)
  - `ASKLLM_API_KEY` - Your API key

## Installation

```bash
cd ms-agent-framework/ConsensusAgent
dotnet build
```

## Usage

```bash
dotnet run <prompt-file> <models-file>
```

### Example

```bash
# From the ConsensusAgent directory
dotnet run ../example-prompt.txt ../models.txt
```

### Input Files

**Prompt File** (`example-prompt.txt`):
A text file containing the question or task for the models to discuss.

**Models File** (`models.txt`):
A newline-delimited list of OpenRouter model names:
```
openai/gpt-5
anthropic/claude-sonnet-4
x-ai/grok-3
deepseek/deepseek-chat-v3.1:free
google/gemini-2.5-pro
```

## Output Files

The application generates two files in the same directory as the prompt file:

1. **Consensus File** (`consensus-YYYYMMDD-HHMMSS.md`):
   - Markdown-formatted final consensus response
   - Includes metadata (timestamp, models used, rounds completed)

2. **Conversation Log** (`conversation-log-YYYYMMDD-HHMMSS.txt`):
   - Detailed log of all rounds and voting results
   - Truncated responses for readability
   - Easy to scan for the discussion flow

## How It Works

1. **Round-based Discussion**: 
   - Each of the 5 models responds to the prompt in Round 1
   - In subsequent rounds, models see previous responses and voting results
   - Models refine their answers based on the emerging consensus

2. **Voting Phase**:
   - After each round (except the last), the first model acts as moderator
   - Identifies common themes, disagreements, and strongest responses
   - Voting result guides the next round

3. **Final Synthesis**:
   - After 5 rounds, a final consensus is generated
   - Incorporates the strongest points from all discussions
   - Formatted as a comprehensive Markdown document

## Configuration

Set environment variables before running:

```powershell
# PowerShell
$env:ASKLLM_API_ENDPOINT = "https://openrouter.ai/api/v1"
$env:ASKLLM_API_KEY = "sk-or-v1-..."
```

```bash
# Bash
export ASKLLM_API_ENDPOINT="https://openrouter.ai/api/v1"
export ASKLLM_API_KEY="sk-or-v1-..."
```

## Project Structure

```
ConsensusAgent/
├── Program.cs              # Main application logic
├── ConsensusAgent.csproj   # Project file with dependencies
├── README.md               # This file
├── example-prompt.txt      # Sample prompt (in parent directory)
└── models.txt              # List of models to use (in parent directory)
```

## Dependencies

- **Microsoft.Agents.AI.OpenAI** (1.0.0-preview.251002.1) - Microsoft Agent Framework OpenAI integration
- **Azure.AI.OpenAI** (2.1.0) - Azure OpenAI SDK for chat completions (compatible with OpenRouter)

## Example Output

```markdown
# Consensus Response

**Generated:** 2025-10-03 14:30:00
**Models Consulted:** 5
**Discussion Rounds:** 5

---

[Final synthesized consensus response incorporating all model perspectives]
```

## Notes

- Total API requests: `number_of_models × 5 rounds` (e.g., 5 models = 25 requests)
- Response times vary based on model speed and API load
- Conversation logs are truncated to 200 characters per response for readability
- The first model in the list is used for voting moderation

## Troubleshooting

**Error: ASKLLM_API_ENDPOINT environment variable not set**
- Ensure environment variables are set in your current shell session

**Error: Prompt file not found**
- Use absolute paths or run from the correct directory
- Check file name spelling

**API Errors**
- Verify your API key is valid and has sufficient credits
- Check that model names match OpenRouter's format (e.g., "openai/gpt-4")
- Ensure endpoint URL is correct and accessible

## License

This project is part of the consensus repository.
