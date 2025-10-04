# PowerShell script to run the Consensus Agent application
# Usage: .\run-consensus.ps1 [-PromptFile <path>] [-ModelsFile <path>] [-OutputId <id>]

param(
    [string]$PromptFile = "$(pwd)\test-prompt.txt",
    [string]$ModelsFile = "$(pwd)\models.txt",
    [string]$OutputId = ""
)

# Check if environment variables are set
if (-not $env:CONSENSUS_API_ENDPOINT) {
    Write-Host "Error: CONSENSUS_API_ENDPOINT environment variable not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:CONSENSUS_API_ENDPOINT = 'https://openrouter.ai/api/v1'" -ForegroundColor Yellow
    exit 1
}

if (-not $env:CONSENSUS_API_KEY) {
    Write-Host "Error: CONSENSUS_API_KEY environment variable not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:CONSENSUS_API_KEY = 'your-api-key'" -ForegroundColor Yellow
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
if ($OutputId) {
    Write-Host "Output ID: $OutputId" -ForegroundColor Cyan
}
Write-Host ""

# Build command-line arguments
$Args = @("--prompt-file", $PromptFile, "--models-file", $ModelsFile)
if ($OutputId) {
    $Args += @("--output-filenames-id", $OutputId)
}

# Run the application
pushd src\Consensus.Console
dotnet run --project Consensus.Console.csproj -- $Args
popd
