using MedicalDiagnosisAPI.Models;

namespace MedicalDiagnosisAPI.Services
{
    public interface IFrontendMedicalService
    {
        Task<EnhancedDiagnosisResponse> AnalyzeSymptomsAsync(SimpleSymptomRequest request);
        Task<TriageResponse> AssessUrgencyAsync(SimpleSymptomRequest request);
        Task<List<SymptomCategory>> GetSymptomCategoriesAsync();
        Task<List<string>> GetSimilarSymptomsAsync(string symptom);
        Task<EnhancedDiagnosisResponse> ComprehensiveHealthAssessmentAsync(HealthAssessmentRequest request);
    }

    public class FrontendMedicalService : IFrontendMedicalService
    {
        private readonly IMedicalMLService _mlService;
        private readonly ILogger<FrontendMedicalService> _logger;

        // Symptom categories for organized display
        private readonly Dictionary<string, List<SymptomOption>> _symptomCategories = new()
        {
            ["Neurological"] = new()
            {
                new() { Name = "headache", DisplayName = "Headache", Description = "Pain in head or neck area", Category = "Neurological" },
                new() { Name = "dizziness", DisplayName = "Dizziness", Description = "Feeling unsteady or lightheaded", Category = "Neurological" },
                new() { Name = "memory_loss", DisplayName = "Memory Loss", Description = "Difficulty remembering things", Category = "Neurological" },
                new() { Name = "confusion", DisplayName = "Confusion", Description = "Difficulty thinking clearly", Category = "Neurological" },
                new() { Name = "tremors", DisplayName = "Tremors", Description = "Involuntary shaking", Category = "Neurological" },
                new() { Name = "numbness", DisplayName = "Numbness", Description = "Loss of sensation", Category = "Neurological" }
            },
            ["Respiratory"] = new()
            {
                new() { Name = "shortness_breath", DisplayName = "Shortness of Breath", Description = "Difficulty breathing", Category = "Respiratory" },
                new() { Name = "cough", DisplayName = "Cough", Description = "Persistent coughing", Category = "Respiratory" },
                new() { Name = "chest_pain", DisplayName = "Chest Pain", Description = "Pain in chest area", Category = "Respiratory" }
            },
            ["Gastrointestinal"] = new()
            {
                new() { Name = "nausea", DisplayName = "Nausea", Description = "Feeling sick to stomach", Category = "Gastrointestinal" },
                new() { Name = "vomiting", DisplayName = "Vomiting", Description = "Throwing up", Category = "Gastrointestinal" },
                new() { Name = "stomach_pain", DisplayName = "Stomach Pain", Description = "Abdominal discomfort", Category = "Gastrointestinal" }
            },
            ["General"] = new()
            {
                new() { Name = "fever", DisplayName = "Fever", Description = "Elevated body temperature", Category = "General" },
                new() { Name = "fatigue", DisplayName = "Fatigue", Description = "Feeling tired or weak", Category = "General" },
                new() { Name = "muscle_weakness", DisplayName = "Muscle Weakness", Description = "Reduced muscle strength", Category = "General" },
                new() { Name = "joint_pain", DisplayName = "Joint Pain", Description = "Pain in joints", Category = "General" }
            },
            ["Cardiovascular"] = new()
            {
                new() { Name = "chest_pain", DisplayName = "Chest Pain", Description = "Pain in chest area", Category = "Cardiovascular" },
                new() { Name = "irregular_heartbeat", DisplayName = "Irregular Heartbeat", Description = "Abnormal heart rhythm", Category = "Cardiovascular" },
                new() { Name = "swelling", DisplayName = "Swelling", Description = "Fluid retention", Category = "Cardiovascular" }
            },
            ["Mental Health"] = new()
            {
                new() { Name = "anxiety", DisplayName = "Anxiety", Description = "Feeling nervous or worried", Category = "Mental Health" },
                new() { Name = "depression", DisplayName = "Depression", Description = "Persistent sadness", Category = "Mental Health" },
                new() { Name = "insomnia", DisplayName = "Insomnia", Description = "Difficulty sleeping", Category = "Mental Health" }
            }
        };

        // Emergency symptoms that require immediate attention
        private readonly HashSet<string> _emergencySymptoms = new()
        {
            "chest_pain", "shortness_breath", "severe_headache", "stroke_symptoms",
            "severe_abdominal_pain", "high_fever", "difficulty_breathing", "loss_of_consciousness"
        };

        public FrontendMedicalService(IMedicalMLService mlService, ILogger<FrontendMedicalService> logger)
        {
            _mlService = mlService;
            _logger = logger;
        }

        public async Task<EnhancedDiagnosisResponse> AnalyzeSymptomsAsync(SimpleSymptomRequest request)
        {
            try
            {
                _logger.LogInformation("Analyzing symptoms for user age {Age} with {SymptomCount} symptoms", 
                    request.Age, request.Symptoms.Count);

                // Convert to internal format
                var internalRequest = ConvertToInternalRequest(request);
                
                // Get ML prediction
                var prediction = await _mlService.PredictDiseaseAsync(internalRequest);

                // Enhance the response
                var response = new EnhancedDiagnosisResponse
                {
                    PrimaryDiagnosis = prediction.PredictedDisease,
                    Confidence = prediction.Confidence,
                    ConfidenceLevel = GetConfidenceLevel(prediction.Confidence),
                    PossibleConditions = prediction.AlternativesDiagnoses,
                    RecommendedActions = GenerateRecommendations(prediction.PredictedDisease, request.Symptoms),
                    Urgency = AssessUrgency(request.Symptoms),
                    Summary = GenerateSummary(prediction.PredictedDisease, prediction.Confidence, request.Symptoms),
                    RequiresImmediateAttention = CheckForEmergencySymptoms(request.Symptoms),
                    Timestamp = DateTime.UtcNow
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing symptoms");
                throw;
            }
        }

        public async Task<TriageResponse> AssessUrgencyAsync(SimpleSymptomRequest request)
        {
            var hasEmergencySymptoms = CheckForEmergencySymptoms(request.Symptoms);
            var severeSymptomsCount = request.Symptoms.Count(s => s.Severity == "severe");
            
            var urgencyLevel = "Low";
            var recommendedAction = "Monitor symptoms and consider scheduling a routine appointment";
            var timeframe = "Within 1-2 weeks";
            var shouldSeekEmergencyCare = false;
            var reasoning = "Symptoms appear to be mild and not immediately concerning.";

            if (hasEmergencySymptoms)
            {
                urgencyLevel = "Critical";
                recommendedAction = "Seek emergency medical care immediately";
                timeframe = "Immediately";
                shouldSeekEmergencyCare = true;
                reasoning = "Symptoms may indicate a serious medical emergency.";
            }
            else if (severeSymptomsCount >= 2)
            {
                urgencyLevel = "High";
                recommendedAction = "Seek medical attention today";
                timeframe = "Within 24 hours";
                reasoning = "Multiple severe symptoms warrant prompt medical evaluation.";
            }
            else if (severeSymptomsCount == 1 || request.Symptoms.Count >= 4)
            {
                urgencyLevel = "Medium";
                recommendedAction = "Schedule an appointment soon";
                timeframe = "Within 2-3 days";
                reasoning = "Symptoms should be evaluated by a healthcare provider.";
            }

            return new TriageResponse
            {
                UrgencyLevel = urgencyLevel,
                RecommendedAction = recommendedAction,
                Timeframe = timeframe,
                ShouldSeekEmergencyCare = shouldSeekEmergencyCare,
                Reasoning = reasoning,
                RedFlags = GetRedFlags(request.Symptoms),
                Instructions = GetInstructions(urgencyLevel)
            };
        }

        public async Task<List<SymptomCategory>> GetSymptomCategoriesAsync()
        {
            return _symptomCategories.Select(kvp => new SymptomCategory
            {
                Name = kvp.Key,
                Description = GetCategoryDescription(kvp.Key),
                Symptoms = kvp.Value
            }).ToList();
        }

        public async Task<List<string>> GetSimilarSymptomsAsync(string symptom)
        {
            // Simple similarity matching - in real implementation, use more sophisticated matching
            var allSymptoms = _symptomCategories.Values
                .SelectMany(symptoms => symptoms.Select(s => s.Name))
                .ToList();

            return allSymptoms
                .Where(s => s.Contains(symptom.ToLower()) || symptom.ToLower().Contains(s))
                .Take(5)
                .ToList();
        }

        public async Task<EnhancedDiagnosisResponse> ComprehensiveHealthAssessmentAsync(HealthAssessmentRequest request)
        {
            var simpleRequest = new SimpleSymptomRequest
            {
                Age = request.Age,
                Gender = request.Gender,
                Symptoms = request.Symptoms
            };

            var basicResponse = await AnalyzeSymptomsAsync(simpleRequest);

            // Enhance with additional health factors
            basicResponse.RecommendedActions.AddRange(GetLifestyleRecommendations(request));
            
            return basicResponse;
        }

        private SymptomRequest ConvertToInternalRequest(SimpleSymptomRequest request)
        {
            // Parse blood pressure from additional info if available
            var (systolicBP, diastolicBP) = ParseBloodPressure(request.AdditionalInfo);
            
            // Normalize symptom names (handle "blurred vision" -> "blurred_vision")
            var normalizedSymptoms = request.Symptoms.Select(s => NormalizeSymptomName(s.Name)).ToList();
            
            _logger.LogInformation("Converted symptoms: {Symptoms} with BP: {Systolic}/{Diastolic}", 
                string.Join(", ", normalizedSymptoms), systolicBP, diastolicBP);
            
            return new SymptomRequest
            {
                Age = request.Age,
                Weight = 70, // Default values for required fields
                GlucoseLevel = 100,
                InsulinLevel = 15,
                SmokingHabit = "no",
                Symptoms = normalizedSymptoms,
                SystolicBP = systolicBP,
                DiastolicBP = diastolicBP,
                HeartRate = 75,
                SpO2 = 98,
                Temperature = 36.5,
                SleepQuality = "good",
                SleepDuration = 7,
                ActivitySteps = 5000,
                ActivityCalories = 2000,
                RespiratoryRate = 16,
                StressLevel = "low"
            };
        }

        private (double systolic, double diastolic) ParseBloodPressure(string? additionalInfo)
        {
            if (string.IsNullOrEmpty(additionalInfo))
                return (120, 80); // Default normal BP
                
            // Look for blood pressure patterns like "150/95", "BP 140/90", "blood pressure 160/100"
            var bpPatterns = new[]
            {
                @"(\d{2,3})\s*[/\-]\s*(\d{2,3})\s*mmHg",
                @"BP\s*[:\s]*(\d{2,3})\s*[/\-]\s*(\d{2,3})",
                @"blood\s*pressure\s*[:\s]*(\d{2,3})\s*[/\-]\s*(\d{2,3})",
                @"(\d{2,3})\s*[/\-]\s*(\d{2,3})"
            };
            
            foreach (var pattern in bpPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(additionalInfo, pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && 
                    double.TryParse(match.Groups[1].Value, out var systolic) &&
                    double.TryParse(match.Groups[2].Value, out var diastolic))
                {
                    _logger.LogInformation("Parsed BP from additional info: {Systolic}/{Diastolic}", systolic, diastolic);
                    return (systolic, diastolic);
                }
            }
            
            return (120, 80); // Default if no BP found
        }

        private string NormalizeSymptomName(string symptomName)
        {
            return symptomName.ToLower()
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace("'", "")
                .Trim();
        }

        private string GetConfidenceLevel(double confidence)
        {
            return confidence switch
            {
                >= 0.7 => "High",
                >= 0.5 => "Medium",
                >= 0.3 => "Low",
                _ => "Very Low - More Information Needed"
            };
        }

        private List<string> GenerateRecommendations(string diagnosis, List<SymptomInput> symptoms)
        {
            var recommendations = new List<string>();

            if (CheckForEmergencySymptoms(symptoms))
            {
                recommendations.Add("Seek immediate medical attention");
                return recommendations;
            }

            // Diagnosis-specific recommendations
            switch (diagnosis.ToLower())
            {
                case var d when d.Contains("diabetes"):
                    recommendations.Add("Monitor blood glucose levels regularly");
                    recommendations.Add("Maintain a balanced diet with controlled carbohydrate intake");
                    recommendations.Add("Stay physically active as appropriate");
                    recommendations.Add("Schedule regular check-ups with an endocrinologist");
                    break;
                    
                case var d when d.Contains("hypertension") || d.Contains("high blood pressure"):
                    recommendations.Add("Monitor blood pressure regularly");
                    recommendations.Add("Reduce sodium intake and maintain a heart-healthy diet");
                    recommendations.Add("Engage in regular moderate exercise");
                    recommendations.Add("Manage stress through relaxation techniques");
                    break;
                    
                case var d when d.Contains("asthma"):
                    recommendations.Add("Avoid known triggers and allergens");
                    recommendations.Add("Keep rescue inhaler accessible at all times");
                    recommendations.Add("Monitor peak flow readings if prescribed");
                    recommendations.Add("Follow your asthma action plan");
                    break;
                    
                case var d when d.Contains("anxiety") || d.Contains("stress"):
                    recommendations.Add("Practice deep breathing and relaxation techniques");
                    recommendations.Add("Maintain regular sleep schedule");
                    recommendations.Add("Consider counseling or therapy if symptoms persist");
                    recommendations.Add("Limit caffeine and alcohol intake");
                    break;
                    
                case var d when d.Contains("insufficient") || d.Contains("unknown"):
                    recommendations.Add("Keep a detailed symptom diary");
                    recommendations.Add("Track when symptoms occur and potential triggers");
                    recommendations.Add("Schedule a comprehensive medical evaluation");
                    recommendations.Add("Consider consulting multiple specialists if needed");
                    break;
                    
                default:
                    // Generic recommendations for unrecognized conditions
                    recommendations.Add("Monitor your symptoms and track any changes");
                    recommendations.Add("Stay hydrated and get adequate rest");
                    
                    if (symptoms.Any(s => s.Name == "fever"))
                    {
                        recommendations.Add("Monitor your temperature regularly");
                        recommendations.Add("Consider over-the-counter fever reducers if appropriate");
                    }

                    if (symptoms.Any(s => s.Name == "headache"))
                    {
                        recommendations.Add("Try relaxation techniques and ensure adequate sleep");
                    }
                    break;
            }

            recommendations.Add("Consult with a healthcare provider if symptoms persist or worsen");
            return recommendations;
        }

        private string AssessUrgency(List<SymptomInput> symptoms)
        {
            if (CheckForEmergencySymptoms(symptoms))
                return "emergency";
            
            if (symptoms.Any(s => s.Severity == "severe"))
                return "urgent";
                
            return "routine";
        }

        private string GenerateSummary(string diagnosis, double confidence, List<SymptomInput> symptoms)
        {
            if (string.IsNullOrEmpty(diagnosis))
            {
                return $"Based on your {symptoms.Count} symptom(s), our analysis suggests monitoring your condition. Consider consulting a healthcare provider if symptoms persist.";
            }

            var confidenceText = confidence switch
            {
                >= 0.8 => "strongly suggests",
                >= 0.6 => "indicates",
                >= 0.4 => "may suggest",
                _ => "shows some possibility of"
            };

            return $"Based on your symptoms, our analysis {confidenceText} {diagnosis}. Confidence level: {GetConfidenceLevel(confidence)}.";
        }

        private bool CheckForEmergencySymptoms(List<SymptomInput> symptoms)
        {
            return symptoms.Any(s => _emergencySymptoms.Contains(s.Name) || s.Severity == "severe");
        }

        private List<string> GetRedFlags(List<SymptomInput> symptoms)
        {
            var redFlags = new List<string>();

            if (symptoms.Any(s => s.Name == "chest_pain"))
                redFlags.Add("Chest pain can indicate serious cardiac conditions");

            if (symptoms.Any(s => s.Name == "shortness_breath"))
                redFlags.Add("Breathing difficulties require prompt evaluation");

            if (symptoms.Any(s => s.Severity == "severe"))
                redFlags.Add("Severe symptoms warrant immediate medical attention");

            return redFlags;
        }

        private List<string> GetInstructions(string urgencyLevel)
        {
            return urgencyLevel switch
            {
                "Critical" => new List<string>
                {
                    "Call emergency services immediately",
                    "Do not drive yourself to the hospital",
                    "Have someone stay with you until help arrives"
                },
                "High" => new List<string>
                {
                    "Contact your healthcare provider today",
                    "Go to urgent care if your doctor is unavailable",
                    "Monitor symptoms closely"
                },
                "Medium" => new List<string>
                {
                    "Schedule an appointment with your healthcare provider",
                    "Keep a symptom diary",
                    "Contact your doctor if symptoms worsen"
                },
                _ => new List<string>
                {
                    "Monitor your symptoms",
                    "Practice self-care measures",
                    "Consider a routine check-up"
                }
            };
        }

        private string GetCategoryDescription(string category)
        {
            return category switch
            {
                "Neurological" => "Symptoms related to the nervous system, brain, and nerves",
                "Respiratory" => "Symptoms affecting breathing and the respiratory system",
                "Gastrointestinal" => "Symptoms related to digestion and the digestive system",
                "General" => "Common symptoms that can occur with various conditions",
                "Cardiovascular" => "Symptoms related to the heart and circulatory system",
                "Mental Health" => "Symptoms affecting mood, thinking, and behavior",
                _ => "Medical symptoms"
            };
        }

        private List<string> GetLifestyleRecommendations(HealthAssessmentRequest request)
        {
            var recommendations = new List<string>();

            if (request.SmokingStatus == "current")
            {
                recommendations.Add("Consider smoking cessation programs");
            }

            if (request.ActivityLevel == "low")
            {
                recommendations.Add("Gradually increase physical activity");
            }

            if (request.Weight.HasValue && request.Height.HasValue)
            {
                var bmi = request.Weight.Value / Math.Pow(request.Height.Value / 100, 2);
                if (bmi > 25)
                {
                    recommendations.Add("Consider discussing weight management with your healthcare provider");
                }
            }

            return recommendations;
        }
    }
}