# consensus
.NET command line and web application to ask multiple models a question, and provide a synthesized consensus answer using a judge.

[Example prompt and model costs](./prompt-model-costs.md)

#### Scripts
A few experimental scripts and prompts are in this repo using [ask-llm](https://github.com/yetanotherchris/ask-llm), to be deleted.

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