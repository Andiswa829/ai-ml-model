# Test script for 5 symptoms
$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    age = 45
    symptoms = @(
        @{ name = "headache"; severity = "moderate" }
        @{ name = "dizziness"; severity = "mild" }
        @{ name = "nausea"; severity = "moderate" }
        @{ name = "fatigue"; severity = "severe" }
        @{ name = "fever"; severity = "moderate" }
    )
} | ConvertTo-Json -Depth 3

Write-Host "Testing with 5 symptoms..."
Write-Host "Request body: $body"

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5051/api/v2/symptoms/analyze" -Method Post -Headers $headers -Body $body
    Write-Host "Response:"
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Error details: $responseBody"
    }
}