# Test script to verify confidence parsing with XML tags

Write-Host "Testing Confidence Score Parsing" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Test cases
$testCases = @(
    @{
        Name = "XML format (preferred)"
        Response = "This is my answer. I'm quite confident about this. <confidence>0.85</confidence>"
        Expected = 0.85
    },
    @{
        Name = "XML format with percentage"
        Response = "This is my answer. <confidence>85</confidence>"
        Expected = 0.85
    },
    @{
        Name = "Legacy: Confidence: format"
        Response = "This is my answer. Confidence: 90%"
        Expected = 0.90
    },
    @{
        Name = "Legacy: Numbered section"
        Response = @"
1. Answer
My answer here

2. Reasoning
My reasoning

3. Confidence Level

**95%**
"@
        Expected = 0.95
    },
    @{
        Name = "Legacy: Embedded in text"
        Response = "I'm providing this answer with 80% confidence based on the data."
        Expected = 0.80
    }
)

Write-Host "Sample test cases for the new XML-based parsing:" -ForegroundColor Yellow
Write-Host ""

foreach ($test in $testCases) {
    Write-Host "Test: $($test.Name)" -ForegroundColor Green
    Write-Host "  Response excerpt: $($test.Response.Substring(0, [Math]::Min(60, $test.Response.Length)))..."
    Write-Host "  Expected confidence: $($test.Expected)" -ForegroundColor White
    Write-Host ""
}

Write-Host "The parser now:" -ForegroundColor Cyan
Write-Host "  1. First tries to extract XML: <confidence>0.85</confidence>" -ForegroundColor White
Write-Host "  2. Handles both decimal (0.85) and percentage (85) formats" -ForegroundColor White
Write-Host "  3. Falls back to legacy regex patterns for backward compatibility" -ForegroundColor White
Write-Host ""

Write-Host "Run your consensus agent to test the new parsing!" -ForegroundColor Green
Write-Host "The prompts now instruct models to use the XML format." -ForegroundColor Green
