# Medical Diagnosis API - Copilot Instructions

This is a C# .NET Web API project that uses ML.NET for medical diagnosis based on symptoms.

## Project Structure
- **Controllers**: API endpoints for symptom input and diagnosis prediction
- **Models**: Data models for symptoms, diagnoses, and ML model training
- **Services**: Business logic and ML model training/prediction services
- **Data**: Medical dataset and ML model files

## Key Technologies
- ASP.NET Core Web API
- ML.NET for machine learning
- Entity Framework Core for data access
- Swagger/OpenAPI for API documentation

## Development Guidelines
- Follow REST API conventions
- Implement proper error handling and validation
- Use dependency injection for services
- Include comprehensive unit tests
- Maintain clear API documentation

## ML Model Considerations
- Train model using provided medical dataset
- Implement symptom preprocessing and feature extraction
- Return confidence scores with predictions
- Handle unknown symptoms gracefully