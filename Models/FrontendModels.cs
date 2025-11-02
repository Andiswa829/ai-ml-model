using Microsoft.ML.Data;

namespace MedicalDiagnosisAPI.Models
{
    /// <summary>
    /// Simplified symptom request for frontend integration
    /// </summary>
    public class SimpleSymptomRequest
    {
        public int Age { get; set; }
        public string Gender { get; set; } = "unspecified";
        public List<SymptomInput> Symptoms { get; set; } = new();
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Individual symptom with severity
    /// </summary>
    public class SymptomInput
    {
        public string Name { get; set; } = string.Empty;
        public string Severity { get; set; } = "mild"; // mild, moderate, severe
        public int Duration { get; set; } = 1; // days
    }

    /// <summary>
    /// Enhanced diagnosis response for frontend
    /// </summary>
    public class EnhancedDiagnosisResponse
    {
        public string PrimaryDiagnosis { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string ConfidenceLevel { get; set; } = string.Empty; // Low, Medium, High
        public List<AlternativeDiagnosis> PossibleConditions { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        public string Urgency { get; set; } = "routine"; // routine, urgent, emergency
        public string Summary { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool RequiresImmediateAttention { get; set; }
    }

    /// <summary>
    /// Symptom category for organized display
    /// </summary>
    public class SymptomCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SymptomOption> Symptoms { get; set; } = new();
    }

    /// <summary>
    /// Individual symptom option with metadata
    /// </summary>
    public class SymptomOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool RequiresSeverity { get; set; } = true;
        public bool RequiresDuration { get; set; } = true;
    }

    /// <summary>
    /// Health assessment request for comprehensive analysis
    /// </summary>
    public class HealthAssessmentRequest
    {
        public int Age { get; set; }
        public string Gender { get; set; } = "unspecified";
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public List<SymptomInput> Symptoms { get; set; } = new();
        public List<string> ExistingConditions { get; set; } = new();
        public List<string> Medications { get; set; } = new();
        public string SmokingStatus { get; set; } = "never";
        public string ActivityLevel { get; set; } = "moderate";
        public string? FamilyHistory { get; set; }
    }

    /// <summary>
    /// Triage response for urgent care assessment
    /// </summary>
    public class TriageResponse
    {
        public string UrgencyLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string RecommendedAction { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public List<string> RedFlags { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public bool ShouldSeekEmergencyCare { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}