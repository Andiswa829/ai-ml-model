using MedicalDiagnosisAPI.Models;

namespace MedicalDiagnosisAPI.Services
{
    public interface IQuestionService
    {
        List<PredefinedQuestion> GetPredefinedQuestions();
        List<PredefinedQuestion> GetFollowUpQuestions(string primaryDiagnosis, List<string> symptoms);
        Dictionary<string, object> AnalyzeQuestionResponses(List<QuestionResponse> responses);
    }

    public class QuestionService : IQuestionService
    {
        private readonly ILogger<QuestionService> _logger;
        
        private readonly List<PredefinedQuestion> _predefinedQuestions = new()
        {
            new PredefinedQuestion 
            { 
                Id = 1, 
                Question = "Do you have a fever?", 
                Category = "Constitutional", 
                ExpectedAnswers = new() { "yes", "no", "mild", "high" },
                IsRequired = true,
                Priority = 1
            },
            new PredefinedQuestion 
            { 
                Id = 2, 
                Question = "Are you experiencing any pain? If so, where?", 
                Category = "Pain", 
                ExpectedAnswers = new() { "no pain", "head", "chest", "abdomen", "back", "joints", "muscles" },
                IsRequired = true,
                Priority = 2
            },
            new PredefinedQuestion 
            { 
                Id = 3, 
                Question = "Have you noticed any changes in your appetite?", 
                Category = "Gastrointestinal", 
                ExpectedAnswers = new() { "normal", "decreased", "increased", "loss of appetite" },
                IsRequired = false,
                Priority = 3
            },
            new PredefinedQuestion 
            { 
                Id = 4, 
                Question = "Are you having trouble sleeping?", 
                Category = "Sleep", 
                ExpectedAnswers = new() { "no", "yes", "insomnia", "frequent waking", "difficulty falling asleep" },
                IsRequired = false,
                Priority = 4
            },
            new PredefinedQuestion 
            { 
                Id = 5, 
                Question = "Have you been in contact with anyone who was sick recently?", 
                Category = "Exposure", 
                ExpectedAnswers = new() { "no", "yes", "family member", "coworker", "unknown" },
                IsRequired = true,
                Priority = 5
            },
            new PredefinedQuestion 
            { 
                Id = 6, 
                Question = "Are you taking any medications currently?", 
                Category = "Medications", 
                ExpectedAnswers = new() { "none", "prescription", "over-the-counter", "supplements" },
                IsRequired = true,
                Priority = 6
            },
            new PredefinedQuestion 
            { 
                Id = 7, 
                Question = "Do you have any known allergies?", 
                Category = "Allergies", 
                ExpectedAnswers = new() { "none", "food", "medication", "environmental", "seasonal" },
                IsRequired = true,
                Priority = 7
            },
            new PredefinedQuestion 
            { 
                Id = 8, 
                Question = "Have you traveled recently?", 
                Category = "Travel", 
                ExpectedAnswers = new() { "no", "domestic", "international", "tropical area" },
                IsRequired = false,
                Priority = 8
            }
        };

        public QuestionService(ILogger<QuestionService> logger)
        {
            _logger = logger;
        }

        public List<PredefinedQuestion> GetPredefinedQuestions()
        {
            return _predefinedQuestions.OrderBy(q => q.Priority).ToList();
        }

        public List<PredefinedQuestion> GetFollowUpQuestions(string primaryDiagnosis, List<string> symptoms)
        {
            var followUpQuestions = new List<PredefinedQuestion>();

            // Disease-specific follow-up questions
            switch (primaryDiagnosis.ToLower())
            {
                case "diabetes":
                    followUpQuestions.AddRange(new[]
                    {
                        new PredefinedQuestion { Id = 101, Question = "How often do you urinate?", Category = "Diabetes", Priority = 1 },
                        new PredefinedQuestion { Id = 102, Question = "Have you experienced unexplained weight loss?", Category = "Diabetes", Priority = 2 },
                        new PredefinedQuestion { Id = 103, Question = "Do you have blurred vision?", Category = "Diabetes", Priority = 3 }
                    });
                    break;

                case "stroke":
                    followUpQuestions.AddRange(new[]
                    {
                        new PredefinedQuestion { Id = 201, Question = "When did the symptoms start? (exact time)", Category = "Stroke", Priority = 1 },
                        new PredefinedQuestion { Id = 202, Question = "Can you smile normally?", Category = "Stroke", Priority = 2 },
                        new PredefinedQuestion { Id = 203, Question = "Can you raise both arms equally?", Category = "Stroke", Priority = 3 }
                    });
                    break;

                case "asthma":
                    followUpQuestions.AddRange(new[]
                    {
                        new PredefinedQuestion { Id = 301, Question = "What triggers your breathing difficulty?", Category = "Respiratory", Priority = 1 },
                        new PredefinedQuestion { Id = 302, Question = "Do you wheeze when breathing?", Category = "Respiratory", Priority = 2 },
                        new PredefinedQuestion { Id = 303, Question = "Do you have a rescue inhaler?", Category = "Respiratory", Priority = 3 }
                    });
                    break;

                case "hypertension":
                    followUpQuestions.AddRange(new[]
                    {
                        new PredefinedQuestion { Id = 401, Question = "Do you monitor your blood pressure regularly?", Category = "Cardiovascular", Priority = 1 },
                        new PredefinedQuestion { Id = 402, Question = "Do you experience headaches frequently?", Category = "Cardiovascular", Priority = 2 },
                        new PredefinedQuestion { Id = 403, Question = "Any family history of heart disease?", Category = "Cardiovascular", Priority = 3 }
                    });
                    break;
            }

            return followUpQuestions.OrderBy(q => q.Priority).ToList();
        }

        public Dictionary<string, object> AnalyzeQuestionResponses(List<QuestionResponse> responses)
        {
            var analysis = new Dictionary<string, object>();
            
            // Extract key information from responses
            var feverResponse = responses.FirstOrDefault(r => r.QuestionId == 1);
            var painResponse = responses.FirstOrDefault(r => r.QuestionId == 2);
            var exposureResponse = responses.FirstOrDefault(r => r.QuestionId == 5);
            var medicationResponse = responses.FirstOrDefault(r => r.QuestionId == 6);
            var allergyResponse = responses.FirstOrDefault(r => r.QuestionId == 7);
            var travelResponse = responses.FirstOrDefault(r => r.QuestionId == 8);

            // Analyze responses for risk factors
            var riskFactors = new List<string>();
            
            if (feverResponse?.Answer.ToLower().Contains("yes") == true || feverResponse?.Answer.ToLower().Contains("high") == true)
            {
                riskFactors.Add("Fever present - suggests infection or inflammation");
                analysis["HasFever"] = true;
            }

            if (painResponse?.Answer.ToLower() != "no pain")
            {
                analysis["PainLocation"] = painResponse?.Answer ?? "unspecified";
                if (painResponse?.Answer.ToLower().Contains("chest") == true)
                {
                    riskFactors.Add("Chest pain - cardiac or pulmonary concern");
                }
            }

            if (exposureResponse?.Answer.ToLower().Contains("yes") == true)
            {
                riskFactors.Add("Recent sick contact - infectious disease risk");
                analysis["InfectionRisk"] = "High";
            }

            if (travelResponse?.Answer.ToLower().Contains("tropical") == true)
            {
                riskFactors.Add("Recent tropical travel - consider tropical diseases");
                analysis["TropicalExposure"] = true;
            }

            analysis["RiskFactors"] = riskFactors;
            analysis["TotalResponses"] = responses.Count;
            analysis["CompletionRate"] = (double)responses.Count / _predefinedQuestions.Count;

            _logger.LogInformation("Analyzed {Count} question responses, identified {RiskCount} risk factors", 
                responses.Count, riskFactors.Count);

            return analysis;
        }
    }
}