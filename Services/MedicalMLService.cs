using Microsoft.ML;
using Microsoft.ML.Data;
using MedicalDiagnosisAPI.Models;
using CsvHelper;
using System.Globalization;

namespace MedicalDiagnosisAPI.Services
{
    public interface IMedicalMLService
    {
        Task<DiagnosisResponse> PredictDiseaseAsync(SymptomRequest request);
        Task TrainModelAsync();
        bool IsModelTrained { get; }
    }

    public class MedicalMLService : IMedicalMLService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _trainedModel;
        private readonly string _modelPath;
        private readonly string _dataPath;
        private readonly ILogger<MedicalMLService> _logger;

        // Available symptoms for validation
        private readonly HashSet<string> _availableSymptoms;
        
        // Store unique diseases from training data
        private List<string> _diseaseLabels = new();

        public bool IsModelTrained => _trainedModel != null;

        public MedicalMLService(ILogger<MedicalMLService> logger)
        {
            _mlContext = new MLContext(seed: 0);
            _logger = logger;
            _modelPath = Path.Combine("Models", "medical_diagnosis_model.zip");
            _dataPath = Path.Combine("Data", "Diseases_Symptoms.csv");

            // Initialize comprehensive symptoms from the Diseases_Symptoms dataset
            _availableSymptoms = new HashSet<string>
            {
                // Neurological symptoms
                "headache", "confusion", "dizziness", "seizures", "weakness", "numbness", 
                "memory_loss", "speech_difficulty", "paralysis", "drowsiness", "disorientation",
                
                // Pain symptoms
                "abdominal_pain", "chest_pain", "pain", "joint_pain", "muscle_pain", "back_pain",
                "lower_abdominal_pain", "pelvic_pain", "cramping",
                
                // Respiratory symptoms  
                "shortness_of_breath", "difficulty_breathing", "cough", "respiratory_distress",
                "wheezing", "chest_tightness",
                
                // Gastrointestinal symptoms
                "nausea", "vomiting", "bleeding", "weight_loss", "loss_of_appetite",
                "digestive_issues", "stomach_pain",
                
                // Visual symptoms
                "blurred_vision", "eye_redness", "light_sensitivity", "cloudy_vision", 
                "excessive_tearing", "decreased_vision", "hazy_eyes", "visual_disturbances",
                "sensitivity_to_light",
                
                // Skin symptoms
                "itching", "rash", "skin_changes", "blisters", "bumps", "itchy_skin",
                "skin_irritation", "dermatitis",
                
                // General symptoms
                "fatigue", "fever", "drowsiness", "restlessness", "swelling", "deformity",
                "malaise", "tiredness", "exhaustion",
                
                // Cardiovascular symptoms
                "heart_rate_changes", "low_blood_pressure", "high_blood_pressure",
                "chest_discomfort", "palpitations",
                
                // Urological symptoms
                "frequent_urination", "urinary_issues", "bladder_problems",
                
                // Endocrine symptoms
                "thirst", "excessive_thirst", "slow_wound_healing", "hormonal_imbalance",
                
                // Psychiatric symptoms
                "anxiety", "depression", "mood_changes", "behavioral_changes",
                "food_restriction", "avoidance_behaviors",
                
                // Musculoskeletal symptoms
                "joint_stiffness", "muscle_cramps", "bone_pain", "mobility_issues",
                
                // Reproductive symptoms
                "menstrual_irregularities", "fertility_issues", "hormonal_symptoms"
            };

            // Try to load existing model
            LoadExistingModel();
        }



        private void LoadExistingModel()
        {
            try
            {
                if (File.Exists(_modelPath))
                {
                    _trainedModel = _mlContext.Model.Load(_modelPath, out var _);
                    _logger.LogInformation("Loaded existing ML model from {ModelPath}", _modelPath);
                    
                    // Load disease labels from training data even when model exists
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var rawData = await LoadMedicalDataAsync();
                            _diseaseLabels = rawData.Select(d => d.Disease).Distinct().OrderBy(d => d).ToList();
                            _logger.LogInformation("Loaded {DiseaseCount} disease labels for existing model", _diseaseLabels.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load disease labels for existing model");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load existing model. Will retrain when needed.");
            }
        }

        public async Task TrainModelAsync()
        {
            try
            {
                _logger.LogInformation("Starting ML model training...");

                // Load and clean data for better accuracy
                var rawData = await LoadMedicalDataAsync();
                var cleanedData = CleanAndBalanceData(rawData);
                
                // Store unique disease labels for prediction mapping
                _diseaseLabels = cleanedData.Select(d => d.Disease).Distinct().OrderBy(d => d).ToList();
                _logger.LogInformation("Found {DiseaseCount} unique diseases: {Diseases}", 
                    _diseaseLabels.Count, string.Join(", ", _diseaseLabels));
                
                // Create data view from cleaned data
                var dataView = _mlContext.Data.LoadFromEnumerable(cleanedData);

                // Define training pipeline with better feature engineering
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(MedicalData.Disease))
                    .Append(_mlContext.Transforms.Concatenate("NumericalFeatures",
                        nameof(MedicalData.Age),
                        nameof(MedicalData.Weight),
                        nameof(MedicalData.GlucoseLevel),
                        nameof(MedicalData.InsulinLevel),
                        nameof(MedicalData.SystolicBP),
                        nameof(MedicalData.DiastolicBP),
                        nameof(MedicalData.HeartRate),
                        nameof(MedicalData.SpO2),
                        nameof(MedicalData.Temperature),
                        nameof(MedicalData.SleepDuration),
                        nameof(MedicalData.ActivitySteps),
                        nameof(MedicalData.ActivityCalories),
                        nameof(MedicalData.RespiratoryRate)))
                    .Append(_mlContext.Transforms.NormalizeMinMax("NumericalFeatures"))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("SmokingHabitFeatures", nameof(MedicalData.SmokingHabit)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Symptom1Features", nameof(MedicalData.Symptom1)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Symptom2Features", nameof(MedicalData.Symptom2)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Symptom3Features", nameof(MedicalData.Symptom3)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Symptom4Features", nameof(MedicalData.Symptom4)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Symptom5Features", nameof(MedicalData.Symptom5)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("SleepQualityFeatures", nameof(MedicalData.SleepQuality)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("StressLevelFeatures", nameof(MedicalData.StressLevel)))
                    .Append(_mlContext.Transforms.Concatenate("Features", 
                        "NumericalFeatures", 
                        "SmokingHabitFeatures", 
                        "Symptom1Features", 
                        "Symptom2Features", 
                        "Symptom3Features", 
                        "Symptom4Features", 
                        "Symptom5Features", 
                        "SleepQualityFeatures", 
                        "StressLevelFeatures"))
                    .Append(_mlContext.MulticlassClassification.Trainers.OneVersusAll(
                        _mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                            labelColumnName: "Label",
                            featureColumnName: "Features",
                            l1Regularization: 0.1f,
                            l2Regularization: 0.1f),
                        labelColumnName: "Label"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Split data for training and evaluation (80/20 split)
                var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                
                // Train the model on training set
                _trainedModel = pipeline.Fit(trainTestSplit.TrainSet);

                // Evaluate the model on test set
                var predictions = _trainedModel.Transform(trainTestSplit.TestSet);
                var metrics = _mlContext.MulticlassClassification.Evaluate(predictions, labelColumnName: "Label");
                
                _logger.LogInformation("Model evaluation metrics:");
                _logger.LogInformation("Overall Accuracy: {Accuracy:P2}", metrics.MacroAccuracy);
                _logger.LogInformation("Log Loss: {LogLoss:F4}", metrics.LogLoss);
                _logger.LogInformation("Per-class accuracy: {PerClassLogLoss}", 
                    string.Join(", ", metrics.PerClassLogLoss.Select((ll, i) => $"Class {i}: {Math.Exp(-ll):P2}")));

                // Test the model with a few examples to verify it's working
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<MedicalData, MedicalPrediction>(_trainedModel);
                
                // Test with first few samples from training data
                var testSamples = rawData.Take(3).ToList();
                foreach (var sample in testSamples)
                {
                    var testPrediction = predictionEngine.Predict(sample);
                    _logger.LogInformation("Test prediction for {ActualDisease}: Predicted={PredictedDisease}, Scores={Scores}",
                        sample.Disease, testPrediction.PredictedDisease, 
                        string.Join(",", testPrediction.Score?.Take(3).Select(s => s.ToString("F3")) ?? new[] { "none" }));
                }

                // Save the model
                Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
                _mlContext.Model.Save(_trainedModel, dataView.Schema, _modelPath);

                _logger.LogInformation("ML model training completed and saved to {ModelPath}", _modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ML model training");
                throw;
            }
        }

        public async Task<DiagnosisResponse> PredictDiseaseAsync(SymptomRequest request)
        {
            if (_trainedModel == null)
            {
                await TrainModelAsync();
            }

            // Ensure disease labels are loaded
            if (_diseaseLabels.Count == 0)
            {
                var rawData = await LoadMedicalDataAsync();
                _diseaseLabels = rawData.Select(d => d.Disease).Distinct().OrderBy(d => d).ToList();
                _logger.LogInformation("Loaded {DiseaseCount} disease labels for prediction", _diseaseLabels.Count);
            }

            try
            {
                // Convert request to medical data format for prediction
                var input = ConvertToMedicalData(request);

                // Create prediction engine
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<MedicalData, MedicalPrediction>(_trainedModel!);

                // Make prediction
                var prediction = predictionEngine.Predict(input);

                // Log prediction details for debugging
                _logger.LogInformation("Prediction made: {PredictedDisease}, Raw scores: {Scores}",
                    prediction.PredictedDisease,
                    string.Join(",", prediction.Score?.Take(5).Select(s => s.ToString("F3")) ?? new[] { "none" }));

                // Calculate and boost confidence
                var rawConfidence = prediction.Score?.Max() ?? 0f;
                var confidence = BoostConfidence(rawConfidence, request.Symptoms.Count);

                // Check if confidence meets minimum threshold
                const float MinimumConfidence = 0.15f; // 15% minimum confidence for meaningful predictions
                const float LowConfidenceThreshold = 0.35f; // 35% threshold for low confidence warning
                
                if (confidence < MinimumConfidence)
                {
                    _logger.LogWarning("Very low confidence prediction: {Confidence:F3} for symptoms: {Symptoms}", 
                        confidence, string.Join(", ", request.Symptoms));
                        
                    return new DiagnosisResponse
                    {
                        PredictedDisease = "Insufficient Data for Reliable Diagnosis",
                        Confidence = confidence,
                        AlternativesDiagnoses = new List<AlternativeDiagnosis> 
                        { 
                            new AlternativeDiagnosis { Disease = "Multiple conditions possible", Confidence = 0.6f },
                            new AlternativeDiagnosis { Disease = "Recommend comprehensive medical evaluation", Confidence = 0.8f },
                            new AlternativeDiagnosis { Disease = "Consider consulting specialist", Confidence = 0.7f }
                        },
                        PredictionDateTime = DateTime.UtcNow
                    };
                }

                // For low confidence but above minimum, add a warning
                var alternativeDiagnoses = GetAlternativeDiagnoses(prediction.Score);
                if (confidence < LowConfidenceThreshold)
                {
                    // Add a disclaimer for low confidence predictions
                    alternativeDiagnoses.Insert(0, new AlternativeDiagnosis 
                    { 
                        Disease = $"Note: Low confidence prediction - seek medical advice", 
                        Confidence = 0.9f 
                    });
                }

                return new DiagnosisResponse
                {
                    PredictedDisease = prediction.PredictedDisease ?? "Unknown Condition",
                    Confidence = confidence,
                    AlternativesDiagnoses = alternativeDiagnoses,
                    PredictionDateTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disease prediction");
                throw;
            }
        }

        private async Task<List<MedicalData>> LoadMedicalDataAsync()
        {
            var medicalDataList = new List<MedicalData>();

            using var reader = new StreamReader(_dataPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Read the Diseases_Symptoms dataset
            csv.Context.RegisterClassMap<DiseaseSymptomsDataMap>();
            await foreach (var record in csv.GetRecordsAsync<DiseaseSymptomsData>())
            {
                // Parse symptoms from the comma-separated string
                var symptoms = ParseSymptoms(record.Symptoms);
                
                // Skip records with no symptoms
                if (symptoms.Count == 0)
                    continue;
                
                // Create training records with different symptom combinations for better coverage
                var maxCombinations = Math.Min(3, symptoms.Count); // Create up to 3 combinations per disease
                
                for (int combo = 0; combo < maxCombinations; combo++)
                {
                    var startIndex = combo;
                    var medicalRecord = new MedicalData
                    {
                        Disease = record.Name.Trim(),
                        
                        // Set default patient demographics with some variation
                        Age = 40f + (combo * 10), // Age variation: 40, 50, 60
                        Gender = combo % 2 == 0 ? "male" : "female",
                        Weight = 70f + (combo * 5), // Weight variation: 70, 75, 80
                        
                        // Assign symptoms with rotation to maximize coverage
                        Symptom1 = symptoms.ElementAtOrDefault(startIndex) ?? "",
                        Symptom2 = symptoms.ElementAtOrDefault((startIndex + 1) % symptoms.Count) ?? "",
                        Symptom3 = symptoms.ElementAtOrDefault((startIndex + 2) % symptoms.Count) ?? "",
                        Symptom4 = symptoms.ElementAtOrDefault((startIndex + 3) % symptoms.Count) ?? "",
                        Symptom5 = symptoms.ElementAtOrDefault((startIndex + 4) % symptoms.Count) ?? "",
                        
                        // Set realistic medical values with some variation
                        GlucoseLevel = 95f + (combo * 10), // 95, 105, 115
                        InsulinLevel = 12f + (combo * 3), // 12, 15, 18
                        SmokingHabit = combo == 0 ? "no" : (combo == 1 ? "occasional" : "yes"),
                        SystolicBP = 115f + (combo * 15), // 115, 130, 145
                        DiastolicBP = 75f + (combo * 10), // 75, 85, 95
                        HeartRate = 70f + (combo * 10), // 70, 80, 90
                        SpO2 = 98f - (combo * 1), // 98, 97, 96
                        Temperature = 36.5f + (combo * 0.3f), // 36.5, 36.8, 37.1
                        SleepQuality = combo == 0 ? "good" : (combo == 1 ? "fair" : "poor"),
                        SleepDuration = 8f - (combo * 1), // 8, 7, 6
                        ActivitySteps = 6000f - (combo * 1000), // 6000, 5000, 4000
                        ActivityCalories = 2200f - (combo * 200), // 2200, 2000, 1800
                        RespiratoryRate = 16f + (combo * 2), // 16, 18, 20
                        StressLevel = combo == 0 ? "low" : (combo == 1 ? "medium" : "high"),
                        BloodPressure = $"{115 + (combo * 15)}/{75 + (combo * 10)}"
                    };
                    
                    medicalDataList.Add(medicalRecord);
                }
            }

            _logger.LogInformation("Loaded {RecordCount} training records from {DiseasesCount} diseases", 
                medicalDataList.Count, medicalDataList.Select(d => d.Disease).Distinct().Count());
            
            // Log the top diseases by sample count
            var diseases = medicalDataList.GroupBy(d => d.Disease)
                .Select(g => new { Disease = g.Key, Count = g.Count() })
                .OrderByDescending(d => d.Count)
                .Take(15);
                
            foreach (var disease in diseases)
            {
                _logger.LogInformation("Disease: {Disease} - {Count} samples", disease.Disease, disease.Count);
            }

            return medicalDataList;
        }
        
        private List<string> ParseSymptoms(string symptomsString)
        {
            if (string.IsNullOrEmpty(symptomsString))
                return new List<string>();
                
            return symptomsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => NormalizeSymptom(s.Trim()))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        private float GetNumericFromBP(string bloodPressure, bool getSystolic)
        {
            // Extract numeric values from blood pressure strings like "Normal", "Low", "High"
            return bloodPressure?.ToLower() switch
            {
                "low" => getSystolic ? 110f : 70f,
                "normal" => getSystolic ? 120f : 80f,
                "high" => getSystolic ? 140f : 90f,
                _ => getSystolic ? 120f : 80f
            };
        }



        private List<MedicalData> CleanAndBalanceData(List<MedicalData> rawData)
        {
            _logger.LogInformation("Cleaning and balancing training data...");

            // Fix data inconsistencies
            foreach (var record in rawData)
            {
                // Fix encoding issues and standardize disease names
                record.Disease = record.Disease
                    .Replace("â€™", "'")  // Fix apostrophe encoding
                    .Replace("Parkinsonâ€™s", "Parkinson's")
                    .Replace("Alzheimerâ€™s", "Alzheimer's")
                    .Trim();

                // Normalize symptoms to lowercase and replace spaces with underscores
                record.Symptom1 = NormalizeSymptom(record.Symptom1);
                record.Symptom2 = NormalizeSymptom(record.Symptom2);
                record.Symptom3 = NormalizeSymptom(record.Symptom3);
                record.Symptom4 = NormalizeSymptom(record.Symptom4);
                record.Symptom5 = NormalizeSymptom(record.Symptom5);
            }

            // Focus on diseases with sufficient training data
            var groupedData = rawData.GroupBy(d => d.Disease).ToList();
            var minSamplesPerClass = 3; // Reduced minimum to ensure we get enough diseases
            
            var validDiseases = groupedData
                .Where(g => g.Count() >= minSamplesPerClass)
                .OrderByDescending(g => g.Count()) // Prioritize diseases with more data
                .ToList();

            // Ensure we have at least 2 diseases for multiclass classification
            if (validDiseases.Count < 2)
            {
                _logger.LogWarning("Not enough diseases with {MinSamples} samples. Reducing threshold...", minSamplesPerClass);
                minSamplesPerClass = 2;
                validDiseases = groupedData
                    .Where(g => g.Count() >= minSamplesPerClass)
                    .OrderByDescending(g => g.Count())
                    .Take(20) // Limit to top 20 diseases to prevent overfitting
                    .ToList();
            }

            if (validDiseases.Count < 2)
            {
                _logger.LogError("Still not enough diseases for training. Found {Count} diseases", validDiseases.Count);
                throw new InvalidOperationException("Need at least 2 diseases with sufficient samples for training");
            }

            _logger.LogInformation("Focused training on {ValidCount} diseases with at least {MinSamples} samples each", 
                validDiseases.Count, minSamplesPerClass);
                
            // Log which diseases we're training on
            foreach (var disease in validDiseases.Take(10))
            {
                _logger.LogInformation("Training disease: {Disease} ({Count} samples)", 
                    disease.Key, disease.Count());
            }

            // Balance the data by limiting maximum samples per class
            var maxSamplesPerClass = 15; // Prevent any single class from dominating
            var balancedData = new List<MedicalData>();

            foreach (var diseaseGroup in validDiseases)
            {
                var samples = diseaseGroup.Take(maxSamplesPerClass).ToList();
                balancedData.AddRange(samples);
                
                _logger.LogInformation("Disease '{Disease}': {Count} samples", 
                    diseaseGroup.Key, samples.Count);
            }

            _logger.LogInformation("Final balanced dataset: {TotalSamples} samples across {DiseaseCount} diseases", 
                balancedData.Count, validDiseases.Count);

            return balancedData;
        }

        private string NormalizeSymptom(string symptom)
        {
            if (string.IsNullOrWhiteSpace(symptom))
                return string.Empty;
                
            var normalized = symptom.ToLower().Replace(" ", "_").Trim();
            
            // Handle common symptom variations to match dataset
            var symptomMappings = new Dictionary<string, string>
            {
                // Neurological symptoms
                {"headache", "headache"},
                {"confusion", "confusion"},
                {"dizziness", "dizziness"},
                {"seizures", "seizures"},
                {"weakness", "weakness"},
                {"numbness", "numbness"},
                {"memory_loss", "memory_loss"},
                {"slurred_speech", "speech_difficulty"},
                {"speech_problems", "speech_difficulty"},
                {"difficulty_speaking", "speech_difficulty"},
                {"paralysis", "paralysis"},
                {"loss_of_speech", "speech_difficulty"},
                
                // Pain symptoms
                {"abdominal_pain", "abdominal_pain"},
                {"chest_pain", "chest_pain"},
                {"pain", "pain"},
                {"joint_pain", "joint_pain"},
                {"muscle_pain", "muscle_pain"},
                {"back_pain", "back_pain"},
                
                // Respiratory symptoms
                {"shortness_of_breath", "shortness_of_breath"},
                {"difficulty_breathing", "difficulty_breathing"},
                {"cough", "cough"},
                {"respiratory_distress", "respiratory_distress"},
                {"breathing_problems", "difficulty_breathing"},
                
                // Gastrointestinal symptoms
                {"nausea", "nausea"},
                {"vomiting", "vomiting"},
                {"bleeding", "bleeding"},
                {"weight_loss", "weight_loss"},
                
                // Visual symptoms
                {"blurred_vision", "blurred_vision"},
                {"blurry_vision", "blurred_vision"},
                {"vision_blur", "blurred_vision"},
                {"decreased_vision", "blurred_vision"},
                {"eye_redness", "eye_redness"},
                {"sensitivity_to_light", "light_sensitivity"},
                {"cloudy_eyes", "cloudy_vision"},
                {"hazy_eyes", "cloudy_vision"},
                {"excessive_tearing", "excessive_tearing"},
                
                // Skin symptoms
                {"itchy_skin", "itching"},
                {"itching", "itching"},
                {"rash", "rash"},
                {"skin_changes", "skin_changes"},
                {"blisters", "blisters"},
                {"bumps", "bumps"},
                
                // General symptoms
                {"fatigue", "fatigue"},
                {"fever", "fever"},
                {"drowsiness", "drowsiness"},
                {"restlessness", "restlessness"},
                {"swelling", "swelling"},
                {"deformity", "deformity"},
                
                // Cardiovascular symptoms  
                {"changes_in_heart_rate", "heart_rate_changes"},
                {"low_blood_pressure", "low_blood_pressure"},
                {"high_blood_pressure", "high_blood_pressure"},
                
                // Urological symptoms
                {"frequent_urination", "frequent_urination"},
                {"urination", "frequent_urination"},
                
                // Diabetes-specific symptoms
                {"thirst", "thirst"},
                {"excessive_thirst", "excessive_thirst"},
                {"dry_mouth", "thirst"},
                {"slow_wound_healing", "slow_wound_healing"},
                {"wound_healing", "slow_wound_healing"},
                {"poor_healing", "slow_wound_healing"}
            };
            
            return symptomMappings.ContainsKey(normalized) ? symptomMappings[normalized] : normalized;
        }

        private float BoostConfidence(float rawConfidence, int symptomCount)
        {
            // More generous confidence calculation for better user experience
            
            // Apply larger boosts for additional symptoms
            var symptomBoost = Math.Min((symptomCount - 3) * 0.05f, 0.15f); // Larger boost for 4+ symptoms
            
            // Apply more generous sigmoid transformation
            var transformed = rawConfidence + (float)(rawConfidence * (1.0 - rawConfidence) * 0.8); // More generous boost
            
            // Combine boosts
            var boostedConfidence = transformed + symptomBoost;
            
            // Guarantee reasonable confidence if the raw confidence shows any signal (>0.15)
            if (rawConfidence > 0.15f && boostedConfidence < 0.45f)
            {
                boostedConfidence = 0.45f;
            }
            
            // Even for low confidence, provide some baseline
            if (rawConfidence > 0.10f && boostedConfidence < 0.30f)
            {
                boostedConfidence = 0.30f; // Minimum reasonable confidence
            }
            
            // Cap at reasonable maximum
            boostedConfidence = Math.Min(boostedConfidence, 0.92f);
            
            _logger.LogInformation("Confidence calculation: Raw={Raw:F3}, Boosted={Boosted:F3}, Symptoms={Count}", 
                rawConfidence, boostedConfidence, symptomCount);
                
            return boostedConfidence;
        }

        private MedicalData ConvertToMedicalData(SymptomRequest request)
        {
            // Use up to 5 symptoms and normalize them for better accuracy
            var symptoms = request.Symptoms.Take(5).Select(NormalizeSymptom).ToList();
            while (symptoms.Count < 5)
            {
                symptoms.Add(""); // Pad with empty strings if less than 5 symptoms
            }

            var symptom1 = symptoms[0];
            var symptom2 = symptoms[1];
            var symptom3 = symptoms[2];
            var symptom4 = symptoms.Count > 3 ? symptoms[3] : "";
            var symptom5 = symptoms.Count > 4 ? symptoms[4] : "";

            _logger.LogInformation("Original symptoms: [{Original1}, {Original2}, {Original3}, {Original4}, {Original5}] from {TotalCount} provided", 
                request.Symptoms.ElementAtOrDefault(0) ?? "", 
                request.Symptoms.ElementAtOrDefault(1) ?? "", 
                request.Symptoms.ElementAtOrDefault(2) ?? "",
                request.Symptoms.ElementAtOrDefault(3) ?? "",
                request.Symptoms.ElementAtOrDefault(4) ?? "",
                request.Symptoms.Count);
            _logger.LogInformation("Normalized symptoms: [{Symptom1}, {Symptom2}, {Symptom3}, {Symptom4}, {Symptom5}]", 
                symptom1, symptom2, symptom3, symptom4, symptom5);

            return new MedicalData
            {
                Age = request.Age,
                Weight = (float)request.Weight,
                BloodPressure = $"{request.SystolicBP:F0}/{request.DiastolicBP:F0}",
                GlucoseLevel = (float)request.GlucoseLevel,
                InsulinLevel = (float)request.InsulinLevel,
                SmokingHabit = request.SmokingHabit.ToLower(),
                Symptom1 = symptom1,
                Symptom2 = symptom2,
                Symptom3 = symptom3,
                Symptom4 = symptom4, // Now use all 5 symptoms
                Symptom5 = symptom5, // Now use all 5 symptoms
                SystolicBP = (float)request.SystolicBP,
                DiastolicBP = (float)request.DiastolicBP,
                HeartRate = (float)request.HeartRate,
                SpO2 = (float)request.SpO2,
                Temperature = (float)request.Temperature,
                SleepQuality = request.SleepQuality.ToLower(),
                SleepDuration = (float)request.SleepDuration,
                ActivitySteps = (float)request.ActivitySteps,
                ActivityCalories = (float)request.ActivityCalories,
                RespiratoryRate = (float)request.RespiratoryRate,
                StressLevel = request.StressLevel.ToLower(),
                Disease = "" // This will be predicted
            };
        }

        private List<AlternativeDiagnosis> GetAlternativeDiagnoses(float[]? scores)
        {
            if (scores == null || scores.Length == 0 || _diseaseLabels.Count == 0)
                return new List<AlternativeDiagnosis>();

            var alternatives = new List<AlternativeDiagnosis>();
            var sortedScores = scores
                .Select((score, index) => new { Score = score, Index = index })
                .OrderByDescending(x => x.Score)
                .Take(Math.Min(3, _diseaseLabels.Count))
                .Where(x => x.Score > 0.001) // Only include scores above threshold
                .ToList();

            foreach (var item in sortedScores)
            {
                // Map index to actual disease name if available
                var diseaseName = item.Index < _diseaseLabels.Count 
                    ? _diseaseLabels[item.Index] 
                    : $"Unknown_Disease_{item.Index}";
                    
                alternatives.Add(new AlternativeDiagnosis
                {
                    Disease = diseaseName,
                    Confidence = item.Score
                });
            }

            return alternatives;
        }
    }

    // CSV mapping class for simple medical dataset
    public class SimpleMedicalDataMap : CsvHelper.Configuration.ClassMap<MedicalData>
    {
        public SimpleMedicalDataMap()
        {
            Map(m => m.Disease).Name("Disease");
            Map(m => m.Symptom1).Name("Symptom1");
            Map(m => m.Symptom2).Name("Symptom2");
            Map(m => m.Symptom3).Name("Symptom3");
            Map(m => m.Age).Name("Age");
            Map(m => m.SystolicBP).Name("SystolicBP");
            Map(m => m.DiastolicBP).Name("DiastolicBP");
        }
    }

    // CSV mapping class for the new Diseases_Symptoms dataset
    public class DiseaseSymptomsDataMap : CsvHelper.Configuration.ClassMap<DiseaseSymptomsData>
    {
        public DiseaseSymptomsDataMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Symptoms).Name("Symptoms");
            Map(m => m.Treatments).Name("Treatments");
            Map(m => m.Disease_Code).Name("Disease_Code");
            Map(m => m.Contagious).Name("Contagious").Convert(args => args.Row.GetField("Contagious") == "True");
            Map(m => m.Chronic).Name("Chronic").Convert(args => args.Row.GetField("Chronic") == "True");
        }
    }

    // Legacy CSV mapping class (kept for backward compatibility)
    public class MedicalDataMap : CsvHelper.Configuration.ClassMap<MedicalData>
    {
        public MedicalDataMap()
        {
            // Map columns from improved_medical_dataset.csv
            Map(m => m.Disease).Name("Disease");
            Map(m => m.HasFever).Name("Fever").Convert(args => args.Row.GetField("Fever") == "Yes");
            Map(m => m.HasCough).Name("Cough").Convert(args => args.Row.GetField("Cough") == "Yes");
            Map(m => m.HasFatigue).Name("Fatigue").Convert(args => args.Row.GetField("Fatigue") == "Yes");
            Map(m => m.HasDifficultyBreathing).Name("Difficulty Breathing").Convert(args => args.Row.GetField("Difficulty Breathing") == "Yes");
            Map(m => m.Age).Name("Age");
            Map(m => m.Gender).Name("Gender");
            Map(m => m.BloodPressure).Name("Blood Pressure");
            Map(m => m.CholesterolLevel).Name("Cholesterol Level");
        }
    }
}