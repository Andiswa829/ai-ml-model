using Microsoft.ML.Data;

namespace MedicalDiagnosisAPI.Models
{
    /// <summary>
    /// Represents medical data used for training the ML model
    /// </summary>
    public class MedicalData
    {
        // Patient demographics
        public float Age { get; set; }     // Changed to float
        public string Gender { get; set; } = string.Empty;
        public float Weight { get; set; } = 70f;

        // Core symptoms (boolean for better ML training)
        public bool HasFever { get; set; }
        public bool HasCough { get; set; }
        public bool HasFatigue { get; set; }
        public bool HasDifficultyBreathing { get; set; }

        // Additional symptoms
        public string Symptom1 { get; set; } = string.Empty;
        public string Symptom2 { get; set; } = string.Empty;
        public string Symptom3 { get; set; } = string.Empty;
        public string Symptom4 { get; set; } = string.Empty;
        public string Symptom5 { get; set; } = string.Empty;

        // Medical indicators
        public string BloodPressure { get; set; } = string.Empty;
        public string CholesterolLevel { get; set; } = string.Empty;
        public float GlucoseLevel { get; set; } = 100f;
        public float InsulinLevel { get; set; } = 15f;
        public string SmokingHabit { get; set; } = "no";
        public float SystolicBP { get; set; }
        public float DiastolicBP { get; set; }
        public float HeartRate { get; set; }
        public float SpO2 { get; set; }
        public float Temperature { get; set; }
        public string SleepQuality { get; set; } = string.Empty;
        public float SleepDuration { get; set; }
        public float ActivitySteps { get; set; }
        public float ActivityCalories { get; set; }
        public float RespiratoryRate { get; set; }
        public string StressLevel { get; set; } = string.Empty;

        // Label for training
        public string Disease { get; set; } = string.Empty;
    }

    /// <summary>
    /// Input model for ML prediction
    /// </summary>
    public class MedicalInput
    {
        public float Age { get; set; }
        public float Weight { get; set; }
        public float GlucoseLevel { get; set; }
        public float InsulinLevel { get; set; }
        public float SmokingHabitEncoded { get; set; }
        public float Symptom1Encoded { get; set; }
        public float Symptom2Encoded { get; set; }
        public float Symptom3Encoded { get; set; }
        public float SystolicBP { get; set; }
        public float DiastolicBP { get; set; }
        public float HeartRate { get; set; }
        public float SpO2 { get; set; }
        public float Temperature { get; set; }
        public float SleepQualityEncoded { get; set; }
        public float SleepDuration { get; set; }
        public float ActivitySteps { get; set; }
        public float ActivityCalories { get; set; }
        public float RespiratoryRate { get; set; }
        public float StressLevelEncoded { get; set; }
    }

    /// <summary>
    /// Output prediction from ML model
    /// </summary>
    public class MedicalPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedDisease { get; set; } = string.Empty;
        
        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    /// <summary>
    /// Model for the new Diseases_Symptoms dataset
    /// </summary>
    public class DiseaseSymptomsData
    {
        public string Name { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatments { get; set; } = string.Empty;
        public string Disease_Code { get; set; } = string.Empty;
        public bool Contagious { get; set; }
        public bool Chronic { get; set; }
    }
}