# PowerShell script to run the Consensus Agent application
# Usage: .\run-consensus.ps1

param(
    [string]$PromptFile = "$(pwd)\test-prompt.txt",
    [string]$ModelsFile = "$(pwd)\models.txt"
)

# Check if environment variables are set
if (-not $env:ASKLLM_API_ENDPOINT) {
    Write-Host "Error: ASKLLM_API_ENDPOINT environment variable not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:ASKLLM_API_ENDPOINT = 'https://openrouter.ai/api/v1'" -ForegroundColor Yellow
    exit 1
}

if (-not $env:ASKLLM_API_KEY) {
    Write-Host "Error: ASKLLM_API_KEY environment variable not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:ASKLLM_API_KEY = 'your-api-key'" -ForegroundColor Yellow
    exit 1
}

# Check if files exist
if (-not (Test-Path $PromptFile)) {
    Write-Host "Error: Prompt file '$PromptFile' not found" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $ModelsFile)) {
    Write-Host "Error: Models file '$ModelsFile' not found" -ForegroundColor Red
    exit 1
}

Write-Host "Starting Consensus Agent..." -ForegroundColor Green
Write-Host "Prompt file: $PromptFile" -ForegroundColor Cyan
Write-Host "Models file: $ModelsFile" -ForegroundColor Cyan
Write-Host ""

# Run the application
pushd src\Consensus.Console
dotnet run --project Consensus.Console.csproj -- $PromptFile $ModelsFile
popd
