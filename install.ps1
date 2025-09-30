scoop bucket add ask-llm https://github.com/yetanotherchris/ask-llm
scoop install ask-llm

# openweb-ui (settings->account to get an API key)
$env:ASKLLM_API_KEY="sk-123"
$env:ASKLLM_API_ENDPOINT="http://localhost:3000/api/"

askllm --model "gemma3:latest" --prompt "Tell me about the moon in 1 sentence"