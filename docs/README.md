# consensus
.NET command line and web application to ask multiple models a question, and provide a synthesized consensus answer using a judge.

<img width="773" height="331" alt="image" src="https://github.com/user-attachments/assets/2dc34183-d185-4294-b4d5-f9125fab44ea" />

## Overview
ReactJS and Tailwind for the frontend, .NET 9 with Microsoft Agent Framework and Quartz for the backend/API.  

The app builds prompts for building a consensus using [prompt templates](https://github.com/yetanotherchris/consensus/tree/main/src/Consensus.Core/Templates) and then the first LLM as the judge (Claude).
Currently there are 6 models hardcoded into the API, and assumes you're using openroute.ai: 
  - anthropic/claude-sonnet-4
  - x-ai/grok-3
  - qwen/qwen3-vl-235b-a22b-thinking
  - alibaba/tongyi-deepresearch-30b-a3b
  - google/gemini-2.5-pro
  - openai/gpt-5

This list will be configurable in the future. Once the consensus 'report' is provided, it's not intended to be available later as the original design was for privacy. But as each prompt has a "runId" it can be changed quite easily.

## Quick start

- Install .NET 9 and NodeJS
- Clone the repo
- Add environment variables:
  - `CONSENSUS_API_ENDPOINT` e.g. https://openrouter.ai/api/v1
  - `CONSENSUS_API_KEY` e.g. `sk-blah-blah`
- `cd .\src\Consensus.Api\` and `dotnet run`
- _(New terminal)_ `cd .\src\Consensus.Web\` and `npm run dev`
- Go to `http://localhost:5173/` in a browser (or the port Vite provides for `npm run dev`)


## Docker

**Build:**

```
docker build -t consensus . ; docker image prune -f
# pwsh: docker build -t consensus . && docker image prune -f
```

**Run:**

```shell
docker run --rm \
    -v $(pwd):/app/data \
    -e CONSENSUS_API_ENDPOINT=https://openrouter.ai/api/v1 \
    -e CONSENSUS_API_KEY=your-key-here \
    -e PROMPT_FILE=/app/data/prompt.txt \
    -e MODELS_FILE=/app/data/models.txt \
    -e OUTPUT_FILENAMES_ID=custom-id \
    consensus
```

```shell
docker run --rm `
    -v "${PWD}:/app/data" `
    -e CONSENSUS_API_ENDPOINT="https://openrouter.ai/api/v1" `
    -e CONSENSUS_API_KEY="your-key-here" `
    -e PROMPT_FILE="/app/data/prompt.txt" `
    -e MODELS_FILE="/app/data/models.txt" `
    -e OUTPUT_FILENAMES_ID="custom-id" `
    consensus
```

#### Scripts
A few experimental scripts and prompts are in this repo using [ask-llm](https://github.com/yetanotherchris/ask-llm), to be deleted.
