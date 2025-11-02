# Medical Diagnosis API

A C# .NET Web API that uses machine learning to predict possible diseases based on patient symptoms and medical data. The API leverages ML.NET for training and prediction using a comprehensive medical dataset.

## Features

- **Symptom-based Diagnosis**: Input patient symptoms and receive AI-powered disease predictions
- **Confidence Scoring**: Get confidence levels for each prediction
- **Alternative Diagnoses**: Receive multiple possible diagnoses ranked by probability
- **Model Training**: Ability to retrain the ML model with new data
- **RESTful API**: Clean, well-documented REST endpoints
- **Swagger Documentation**: Interactive API documentation
- **Comprehensive Validation**: Input validation for medical parameters

## API Endpoints

### POST /api/diagnosis/predict
Predicts possible diseases based on symptoms and medical data.

**Request Body:**
```json
{
  "age": 45,
  "weight": 70.5,
  "glucoseLevel": 120,
  "insulinLevel": 25,
  "smokingHabit": "no",
  "symptoms": ["headache", "dizziness", "nausea"],
  "systolicBP": 140,
  "diastolicBP": 90,
  "heartRate": 80,
  "spO2": 98,
  "temperature": 37.2,
  "sleepQuality": "good",
  "sleepDuration": 7.5,
  "activitySteps": 8000,
  "activityCalories": 2000,
  "respiratoryRate": 16,
  "stressLevel": "moderate"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Diagnosis prediction completed successfully",
  "data": {
    "predictedDisease": "Hypertension",
    "confidence": 0.85,
    "alternativesDiagnoses": [
      {
        "disease": "Cardiovascular Disease",
        "confidence": 0.72
      }
    ],
    "predictionDateTime": "2025-10-28T12:00:00Z"
  }
}
```

### GET /api/diagnosis/symptoms
Returns available symptoms for diagnosis.

### GET /api/diagnosis/status
Returns the current status of the ML model.

### POST /api/diagnosis/train
Triggers ML model training/retraining.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio Code (recommended)
- C# Dev Kit extension for VS Code

### Installation

1. Clone the repository
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

### Development

1. The Swagger UI is available at the root URL when running in development mode
2. The ML model will be automatically trained on first prediction request
3. Model files are stored in the `Models` directory
4. Dataset is located in the `Data` directory

## Available Symptoms

The API supports the following symptoms for diagnosis:

- memory_loss, confusion, disorientation
- tremors, stiff_muscles, speech_issue
- blurred_vision, fatigue, frequent_urination
- joint_pain, rash, fever
- headache, dizziness, nausea
- numbness, muscle_weakness, speech_difficulty
- restlessness, anxiety, insomnia
- depression, chest_pain, shortness_breath

## Medical Parameters

The API accepts various medical parameters:

- **Vital Signs**: Blood pressure, heart rate, temperature, SpO2, respiratory rate
- **Metabolic**: Glucose level, insulin level
- **Lifestyle**: Smoking habit, sleep quality/duration, activity metrics
- **Demographics**: Age, weight
- **Psychological**: Stress level

## Model Information

- **Framework**: ML.NET 3.0.1
- **Algorithm**: SDCA Maximum Entropy (Multi-class Classification)
- **Features**: 19 input features including demographics, symptoms, and vital signs
- **Output**: Disease classification with confidence scores

## Architecture

```
├── Controllers/           # API controllers
│   └── DiagnosisController.cs
├── Models/               # Data models
│   ├── ApiModels.cs     # API request/response models
│   └── MedicalModels.cs # ML training models
├── Services/            # Business logic
│   └── MedicalMLService.cs # ML service implementation
├── Data/               # Dataset storage
│   └── medical_dataset.csv
└── Models/             # Trained ML models (generated)
    └── medical_diagnosis_model.zip
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is for educational and research purposes. Please ensure compliance with medical data regulations when using in production environments.

## Disclaimer

This API is for demonstration and educational purposes only. It should not be used for actual medical diagnosis. Always consult qualified healthcare professionals for medical advice and diagnosis.