$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$model = "anthropic/claude-sonnet-4.5"
$prompt = Get-Content "prompts/dialogues/super-ai.txt"
$outputFilename = "chatlog.md";
$initialPromptFilename = "initial-prompt.txt"
$prompt > $initialPromptFilename

Write-Output "Prompting...";
Write-Output "Answer this question: $prompt  `n  `n" >> "$outputFilename";
$output = & askllm --input-file "temp-prompt.txt" --color "" --model "$model";

$total=50;
for ($i = 1; $i -lt $total; $i++) 
{ 
    Write-Output "request $i ...";

    $outputFileContents = Get-Content "$outputFilename"
    $fullPrompt = "$systemPrompt`n`n$output`n`n$lastOutput"
    Write-Output "$fullPrompt" > temp-prompt.txt

    $lastOutput = & askllm --input-file "temp-prompt.txt" --color ""  --model "$model";
    Write-Output "$lastOutput  `n  `n" >> "$outputFilename"; 
}