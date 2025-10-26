# consensus
.NET command line and web application to ask multiple models a question, and provide a synthesized consensus answer using a judge.

## Overview
ReactJS and Tailwind for the frontend, .NET 9 with Microsoft Agent Framework and Quartz for the backend/API.  

The app builds prompts for building a consensus using [prompt templates](https://github.com/yetanotherchris/consensus/tree/main/src/Consensus.Core/Templates) and then the first LLM as the judge.

## Quickstart

`docker run todo`

## Docs
- [Example prompt and model costs](./prompt-model-costs.md)

## Screenshot
<img width="773" height="331" alt="image" src="https://github.com/user-attachments/assets/2dc34183-d185-4294-b4d5-f9125fab44ea" />


## Local development

- Install .NET 9 and NodeJS
- Clone the repo
- Add environment variables:
  - `CONSENSUS_API_ENDPOINT` e.g. https://openrouter.ai/api/v1
  - `CONSENSUS_API_KEY` e.g. `sk-blah-blah`
- `cd .\src\Consensus.Api\` and `dotnet run`
- _(New terminal)_ `cd .\src\Consensus.Web\` and `npm run dev`
- Go to `http://localhost:5173/` in a browser (or the port Vite provides for `npm run dev`)