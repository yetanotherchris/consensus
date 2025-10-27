Remove-Job -name consensus-api -Force -ErrorAction SilentlyContinue | Out-Null
Remove-Job -name consensus-web -Force -ErrorAction SilentlyContinue | Out-Null

Push-Location ./src/Consensus.Api
Start-Job -Name "consensus-api" -ScriptBlock { dotnet run }
Pop-Location

Push-Location  ./src/Consensus.Web
Start-Job -Name "consensus-web" -ScriptBlock { npm run dev }
Pop-Location

Write-Host "Waiting for jobs to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

Write-Host "----"
Write-Host "Use 'Receive-Job -Name consensus-api' to see logs" -ForegroundColor Magenta
Write-Host "Use 'Remove-Job -Name consensus-api' to finish, or get-job | remove-job" -ForegroundColor Magenta

$output = Receive-Job -Name consensus-web -Keep
if ($output) {
    Write-Host $output
} else {
    Write-Host "No output yet, give it a moment..."
}