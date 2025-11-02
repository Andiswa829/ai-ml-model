# Frontend Integration Guide

## Medical Diagnosis API - React Frontend Integration

This API has been enhanced specifically for integration with React-based symptom checker applications. It provides simplified, frontend-friendly endpoints optimized for modern web applications.

## Base URL
```
http://localhost:5051/api/v2/symptoms
```

## Frontend-Optimized Endpoints

### 1. Health Check
**GET** `/health`

Returns API status and available services.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-10-28T12:00:00Z",
  "version": "2.0",
  "services": {
    "symptomAnalysis": "Available",
    "triageAssessment": "Available",
    "symptomCategories": "Available"
  }
}
```

### 2. Get Symptom Categories
**GET** `/categories`

Returns organized symptom categories for UI display.

**Response:**
```json
{
  "success": true,
  "message": "Symptom categories retrieved successfully",
  "data": [
    {
      "name": "Neurological",
      "description": "Symptoms related to the nervous system, brain, and nerves",
      "symptoms": [
        {
          "name": "headache",
          "displayName": "Headache",
          "description": "Pain in head or neck area",
          "category": "Neurological",
          "requiresSeverity": true,
          "requiresDuration": true
        }
      ]
    }
  ]
}
```

### 3. Search Symptoms (Autocomplete)
**GET** `/search?query={searchTerm}`

Returns symptom suggestions for autocomplete functionality.

**Example:** `/search?query=head`

**Response:**
```json
{
  "success": true,
  "message": "Symptom suggestions retrieved",
  "data": ["headache", "head_injury", "head_congestion"]
}
```

### 4. Simple Symptom Analysis
**POST** `/analyze`

Main endpoint for symptom analysis - optimized for frontend use.

**Request Body:**
```json
{
  "age": 35,
  "gender": "female",
  "symptoms": [
    {
      "name": "headache",
      "severity": "moderate",
      "duration": 3
    },
    {
      "name": "nausea",
      "severity": "mild",
      "duration": 2
    }
  ],
  "additionalInfo": "Started after stressful work week"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Symptom analysis completed successfully",
  "data": {
    "primaryDiagnosis": "Tension Headache",
    "confidence": 0.75,
    "confidenceLevel": "High",
    "possibleConditions": [
      {
        "disease": "Migraine",
        "confidence": 0.65
      }
    ],
    "recommendedActions": [
      "Monitor your symptoms and track any changes",
      "Try relaxation techniques and ensure adequate sleep",
      "Consult with a healthcare provider if symptoms persist"
    ],
    "urgency": "routine",
    "summary": "Based on your symptoms, our analysis indicates Tension Headache. Confidence level: High.",
    "requiresImmediateAttention": false,
    "timestamp": "2025-10-28T12:00:00Z"
  }
}
```

### 5. Triage Assessment
**POST** `/triage`

Assesses urgency level for medical triage.

**Request Body:**
```json
{
  "age": 45,
  "gender": "male",
  "symptoms": [
    {
      "name": "chest_pain",
      "severity": "severe",
      "duration": 1
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Triage assessment completed",
  "data": {
    "urgencyLevel": "Critical",
    "recommendedAction": "Seek emergency medical care immediately",
    "timeframe": "Immediately",
    "shouldSeekEmergencyCare": true,
    "reasoning": "Symptoms may indicate a serious medical emergency.",
    "redFlags": ["Chest pain can indicate serious cardiac conditions"],
    "instructions": [
      "Call emergency services immediately",
      "Do not drive yourself to the hospital",
      "Have someone stay with you until help arrives"
    ]
  }
}
```

### 6. Comprehensive Health Assessment
**POST** `/comprehensive`

Detailed health assessment including lifestyle factors.

**Request Body:**
```json
{
  "age": 28,
  "gender": "female",
  "weight": 65,
  "height": 165,
  "symptoms": [
    {
      "name": "fatigue",
      "severity": "moderate",
      "duration": 14
    }
  ],
  "existingConditions": ["hypothyroidism"],
  "medications": ["levothyroxine"],
  "smokingStatus": "never",
  "activityLevel": "moderate",
  "familyHistory": "Autoimmune conditions in family"
}
```

## Frontend Integration Tips

### 1. Symptom Input Component
```typescript
interface SymptomInput {
  name: string;
  severity: 'mild' | 'moderate' | 'severe';
  duration: number; // days
}

interface SymptomRequest {
  age: number;
  gender?: string;
  symptoms: SymptomInput[];
  additionalInfo?: string;
}
```

### 2. API Service Example
```typescript
class MedicalAPIService {
  private baseURL = 'http://localhost:5051/api/v2/symptoms';

  async getSymptomCategories() {
    const response = await fetch(`${this.baseURL}/categories`);
    return response.json();
  }

  async analyzeSymptoms(request: SymptomRequest) {
    const response = await fetch(`${this.baseURL}/analyze`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    });
    return response.json();
  }

  async searchSymptoms(query: string) {
    const response = await fetch(`${this.baseURL}/search?query=${query}`);
    return response.json();
  }
}
```

### 3. Urgency Level Handling
```typescript
const getUrgencyColor = (urgency: string) => {
  switch (urgency) {
    case 'Critical': return 'red';
    case 'High': return 'orange';
    case 'Medium': return 'yellow';
    case 'Low': return 'green';
    default: return 'gray';
  }
};
```

### 4. Error Handling
```typescript
const handleAPIError = (error: any) => {
  if (error.response?.data?.errors) {
    return error.response.data.errors.join(', ');
  }
  return 'An unexpected error occurred. Please try again.';
};
```

## Validation Rules

### Age
- Range: 0-150 years
- Required field

### Symptoms
- Minimum: 1 symptom
- Maximum: 10 symptoms
- Each symptom must have valid severity: 'mild', 'moderate', 'severe'
- Duration: 0-365 days

### Gender (Optional)
- Values: 'male', 'female', 'unspecified'

## Response Codes

- **200 OK**: Successful analysis
- **400 Bad Request**: Invalid input data
- **500 Internal Server Error**: Server processing error

## Security Considerations

1. **Rate Limiting**: Implement client-side rate limiting
2. **Input Sanitization**: Sanitize all user inputs
3. **HTTPS**: Use HTTPS in production
4. **API Keys**: Consider implementing API key authentication for production

## Performance Optimization

1. **Caching**: Cache symptom categories locally
2. **Debouncing**: Debounce search queries
3. **Loading States**: Show loading indicators during API calls
4. **Error Boundaries**: Implement React error boundaries

## Sample React Components

### SymptomSelector Component
```tsx
const SymptomSelector = ({ onSymptomsChange }) => {
  const [categories, setCategories] = useState([]);
  const [selectedSymptoms, setSelectedSymptoms] = useState([]);

  useEffect(() => {
    fetch('/api/v2/symptoms/categories')
      .then(res => res.json())
      .then(data => setCategories(data.data));
  }, []);

  // Component implementation...
};
```

### DiagnosisResults Component
```tsx
const DiagnosisResults = ({ diagnosis }) => {
  const urgencyColor = getUrgencyColor(diagnosis.urgency);
  
  return (
    <div className="diagnosis-results">
      <h2>{diagnosis.primaryDiagnosis}</h2>
      <div className={`confidence-badge ${diagnosis.confidenceLevel.toLowerCase()}`}>
        Confidence: {diagnosis.confidenceLevel}
      </div>
      <p>{diagnosis.summary}</p>
      
      {diagnosis.requiresImmediateAttention && (
        <div className="emergency-alert">
          ⚠️ Seek immediate medical attention
        </div>
      )}
      
      <div className="recommendations">
        <h3>Recommendations</h3>
        <ul>
          {diagnosis.recommendedActions.map((action, index) => (
            <li key={index}>{action}</li>
          ))}
        </ul>
      </div>
    </div>
  );
};
```

This enhanced API provides everything your React symptom checker frontend needs for a professional, user-friendly medical diagnosis experience!