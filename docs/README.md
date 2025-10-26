# consensus
consensus is a web application (+command line tool) to ask multiple models a question, and provide a synthesized consensus answer using one model as the judge.

## Overview
ReactJS and Tailwind for the frontend, .NET 9 with Microsoft Agent Framework and Quartz for the backend/API.  

The app builds prompts for building a consensus using [prompt templates](https://github.com/yetanotherchris/consensus/tree/main/src/Consensus.Core/Templates) and then the first LLM as the judge.

## Screenshot
<img width="773" height="331" alt="image" src="https://github.com/user-attachments/assets/2dc34183-d185-4294-b4d5-f9125fab44ea" />

## Quickstart

```
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key-here" \
  -e Consensus__Models__0="openai/gpt-4" \
  -e Consensus__Models__1="anthropic/claude-3-opus" \
  -e Consensus__Models__2="microsoft/phi-4" \
  -e Consensus__Models__3="google/gemini-2.5-flash" \
  -v $(pwd)/output:/app/output \
  --name consensus \
  ghcr.io/yetanotherchris/consensus:latest
```

Now go to http://localhost:8585/  

Or use Docker compose:

```
services:
  consensus:
    image: ghcr.io/yetanotherchris/consensus:latest
    container_name: consensus
    ports:
      - "8085:8080"
    environment:
      - Consensus__ApiEndpoint=https://openrouter.ai/api/v1
      - Consensus__ApiKey=your-api-key-here
      - Consensus__Models__0=openai/gpt-4
      - Consensus__Models__1=anthropic/claude-3-opus
      - Consensus__Models__2=google/gemini-pro
      - Consensus__Models__3=microsoft/phi-4
    volumes:
      - ./output:/app/output
    restart: unless-stopped
```

[Example prompt and model costs](./prompt-model-costs.md)


## Local development

- Install .NET 9 and NodeJS
- Clone the repo
- Add environment variables:
  - `CONSENSUS_API_ENDPOINT` e.g. https://openrouter.ai/api/v1
  - `CONSENSUS_API_KEY` e.g. `sk-blah-blah`
- `cd .\src\Consensus.Api\` and `dotnet run`
- _(New terminal)_ `cd .\src\Consensus.Web\` and `npm run dev`
- Go to `http://localhost:5173/` in a browser (or the port Vite provides for `npm run dev`)
