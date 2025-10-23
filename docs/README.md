# consensus
.NET command line and web application to ask multiple models a question, and provide a synthesized consensus answer using a judge.

<img width="773" height="331" alt="image" src="https://github.com/user-attachments/assets/2dc34183-d185-4294-b4d5-f9125fab44ea" />

ReactJS and Tailwind for the frontend, .NET 9 with Microsoft Agent Framework and Quartz for the backend/API.

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
