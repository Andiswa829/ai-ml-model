namespace MedicalDiagnosisAPI.Models
{
    /// <summary>
    /// Request model for symptom input from API clients
    /// </summary>
    public class SymptomRequest
    {
        public int Age { get; set; }
        public double Weight { get; set; }
        public double GlucoseLevel { get; set; }
        public double InsulinLevel { get; set; }
        public string SmokingHabit { get; set; } = "no";
        public List<string> Symptoms { get; set; } = new();
        public double SystolicBP { get; set; }
        public double DiastolicBP { get; set; }
        public double HeartRate { get; set; }
        public double SpO2 { get; set; }
        public double Temperature { get; set; }
        public string SleepQuality { get; set; } = "good";
        public double SleepDuration { get; set; }
        public double ActivitySteps { get; set; }
        public double ActivityCalories { get; set; }
        public double RespiratoryRate { get; set; }
        public string StressLevel { get; set; } = "low";
    }

    /// <summary>
    /// Response model for diagnosis predictions
    /// </summary>
    public class DiagnosisResponse
    {
        public string PredictedDisease { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<AlternativeDiagnosis> AlternativesDiagnoses { get; set; } = new();
        public DateTime PredictionDateTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Alternative diagnosis with confidence score
    /// </summary>
    public class AlternativeDiagnosis
    {
        public string Disease { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    /// <summary>
    /// API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}