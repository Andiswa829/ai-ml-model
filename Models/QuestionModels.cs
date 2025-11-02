using MedicalDiagnosisAPI.Models;

namespace MedicalDiagnosisAPI.Models
{
    public class PredefinedQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> ExpectedAnswers { get; set; } = new();
        public bool IsRequired { get; set; }
        public int Priority { get; set; } // 1 = highest priority
    }

    public class QuestionResponse
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class EnhancedSymptomRequest : SimpleSymptomRequest
    {
        public List<QuestionResponse> QuestionResponses { get; set; } = new();
        public bool HasFever { get; set; }
        public string? PainLocation { get; set; }
        public bool AppetiteChanged { get; set; }
        public bool SleepTrouble { get; set; }
        public bool RecentSickContact { get; set; }
        public List<string> CurrentMedications { get; set; } = new();
        public List<string> KnownAllergies { get; set; } = new();
        public bool RecentTravel { get; set; }
        public string? TravelLocation { get; set; }
    }

    public class DetailedDiagnosisResponse : EnhancedDiagnosisResponse
    {
        public List<PredefinedQuestion> RecommendedFollowUpQuestions { get; set; } = new();
        public Dictionary<string, string> RiskFactors { get; set; } = new();
        public string DetailedAnalysis { get; set; } = string.Empty;
        public List<string> RedFlags { get; set; } = new();
    }

    public class FollowUpQuestionRequest
    {
        public string PrimaryDiagnosis { get; set; } = string.Empty;
        public List<string> Symptoms { get; set; } = new();
    }
}