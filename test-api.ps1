# Test the Medical Diagnosis API

$baseUrl = "http://localhost:5051"

# Test 1: Simple symptom analysis with respiratory symptoms (should not be General Anxiety)
Write-Host "Testing respiratory symptoms (fever, cough, chest pain)..."
$body1 = @{
    age = 45
    gender = "male"
    symptoms = @(
        @{
            name = "fever"
            severity = "high"
            duration = 3
        },
        @{
            name = "cough"
            severity = "moderate"
            duration = 5
        },
        @{
            name = "chest pain"
            severity = "moderate"
            duration = 2
        }
    )
    additionalInfo = "Started 3 days ago with fever"
} | ConvertTo-Json -Depth 3

try {
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/symptoms/analyze" -Method POST -ContentType "application/json" -Body $body1
    Write-Host "Result 1:" -ForegroundColor Green
    $response1 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error in Test 1: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n" + "="*50 + "`n"

# Test 2: Neurological symptoms (should predict Parkinson's or similar)
Write-Host "Testing neurological symptoms (tremors, stiff muscles)..."
$body2 = @{
    age = 65
    gender = "male"
    symptoms = @(
        @{
            name = "tremors"
            severity = "moderate"
            duration = 30
        },
        @{
            name = "stiff muscles"
            severity = "moderate"
            duration = 60
        }
    )
    additionalInfo = "Progressive symptoms over months"
} | ConvertTo-Json -Depth 3

try {
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/symptoms/analyze" -Method POST -ContentType "application/json" -Body $body2
    Write-Host "Result 2:" -ForegroundColor Green
    $response2 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error in Test 2: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n" + "="*50 + "`n"

# Test 3: Cardiovascular symptoms
Write-Host "Testing cardiovascular symptoms (chest pain, shortness of breath)..."
$body3 = @{
    age = 55
    gender = "female"
    symptoms = @(
        @{
            name = "chest pain"
            severity = "high"
            duration = 1
        },
        @{
            name = "shortness of breath"
            severity = "high"
            duration = 2
        }
    )
    additionalInfo = "Sudden onset during physical activity"
} | ConvertTo-Json -Depth 3

try {
    $response3 = Invoke-RestMethod -Uri "$baseUrl/api/symptoms/analyze" -Method POST -ContentType "application/json" -Body $body3
    Write-Host "Result 3:" -ForegroundColor Green
    $response3 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error in Test 3: $($_.Exception.Message)" -ForegroundColor Red
}