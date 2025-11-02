# Test the Medical Diagnosis API with hypertension symptoms
$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    age = 55
    gender = "female"
    weight = 70
    systolicBP = 150
    diastolicBP = 95
    heartRate = 85
    spO2 = 98
    temperature = 36.5
    glucoseLevel = 100
    insulinLevel = 15
    smokingHabit = "no"
    sleepQuality = "fair"
    sleepDuration = 7
    activitySteps = 3000
    activityCalories = 1800
    respiratoryRate = 18
    stressLevel = "high"
    symptoms = @(
        "headache",
        "dizziness", 
        "blurred vision",
        "chest pain",
        "shortness of breath"
    )
} | ConvertTo-Json -Depth 3

Write-Host "Testing Medical Diagnosis API with hypertension symptoms..." -ForegroundColor Yellow
Write-Host "Request body:" -ForegroundColor Green
Write-Host $body

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5051/api/diagnosis/predict" -Method Post -Body $body -Headers $headers
    Write-Host "`nSUCCESS - Diagnosis completed!" -ForegroundColor Green
    Write-Host "Primary Diagnosis: $($response.data.predictedDisease)" -ForegroundColor Cyan
    Write-Host "Confidence: $($response.data.confidence * 100)%" -ForegroundColor Cyan
    
    if ($response.data.alternativesDiagnoses) {
        Write-Host "`nAlternative Diagnoses:" -ForegroundColor Yellow
        foreach ($alt in $response.data.alternativesDiagnoses) {
            Write-Host "  - $($alt.disease): $([math]::Round($alt.confidence * 100, 2))%" -ForegroundColor White
        }
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}