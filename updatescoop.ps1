param(
        [Parameter(Mandatory=$true)]
        [String]
        $version
)

$search = "*publish/consensus.exe"
$consensusFile = Get-ChildItem -Path "./bin/Release/" -Force -Recurse -File | Where-Object { $_.FullName -like $search }
$filePath = $consensusFile[0].FullName

Write-Output "File found: $filePath, getting hash..."
$hash = (Get-FileHash -Path $filePath -Algorithm SHA256).Hash
Write-Output "Hash: $hash"

Write-Output "Creating consensus.json for version $version..."
Write-Output "{"
Write-Output "  \"version\": \"$version\"," 
Write-Output "  \"architecture\": {"
Write-Output "      \"64bit\": {"
Write-Output "          \"url\": \"https://github.com/yetanotherchris/consensus/releases/download/v$version/consensus.exe\"," 
Write-Output "          \"bin\": ["
Write-Output "              \"consensus.exe\""
Write-Output "          ],"
Write-Output "          \"hash\": \"$hash\""
Write-Output "      }"
Write-Output "  },"
Write-Output "  \"homepage\": \"https://github.com/yetanotherchris/consensus\"," 
Write-Output "  \"license\": \"MIT License\"," 
Write-Output "  \"description\": \"A console app that aggregates multiple LLM responses\""
Write-Output "}" | Out-File -FilePath "consensus.json" -Encoding utf8
