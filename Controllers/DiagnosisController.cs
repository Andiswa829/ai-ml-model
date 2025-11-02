using Microsoft.AspNetCore.Mvc;
using MedicalDiagnosisAPI.Models;
using MedicalDiagnosisAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace MedicalDiagnosisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosisController : ControllerBase
    {
        private readonly IMedicalMLService _mlService;
        private readonly ILogger<DiagnosisController> _logger;

        public DiagnosisController(IMedicalMLService mlService, ILogger<DiagnosisController> logger)
        {
            _mlService = mlService;
            _logger = logger;
        }

        /// <summary>
        /// Predicts possible diseases based on provided symptoms and medical data
        /// </summary>
        /// <param name="request">Patient symptoms and medical information</param>
        /// <returns>Diagnosis prediction with confidence scores</returns>
        [HttpPost("predict")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<DiagnosisResponse>>> PredictDisease(
            [FromBody] SymptomRequest request)
        {
            try
            {
                // Validate request
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = validationResult.Errors
                    });
                }

                _logger.LogInformation("Processing diagnosis request for age {Age} with {SymptomCount} symptoms", 
                    request.Age, request.Symptoms.Count);

                // Get prediction
                var prediction = await _mlService.PredictDiseaseAsync(request);

                return Ok(new ApiResponse<DiagnosisResponse>
                {
                    Success = true,
                    Message = "Diagnosis prediction completed successfully",
                    Data = prediction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing diagnosis request");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing the diagnosis request",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Trains or retrains the ML model with the latest data
        /// </summary>
        /// <returns>Training status</returns>
        [HttpPost("train")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> TrainModel()
        {
            try
            {
                _logger.LogInformation("Starting ML model training");
                
                await _mlService.TrainModelAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "ML model training completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model training");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during model training",
                    Errors = new List<string> { "Training failed" }
                });
            }
        }

        /// <summary>
        /// Gets the current status of the ML model
        /// </summary>
        /// <returns>Model status information</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public ActionResult<ApiResponse<object>> GetModelStatus()
        {
            var status = new
            {
                IsModelTrained = _mlService.IsModelTrained,
                LastUpdated = DateTime.UtcNow,
                ModelVersion = "1.0.0"
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Model status retrieved successfully",
                Data = status
            });
        }

        /// <summary>
        /// Gets available symptoms that can be used for diagnosis
        /// </summary>
        /// <returns>List of available symptoms</returns>
        [HttpGet("symptoms")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public ActionResult<ApiResponse<List<string>>> GetAvailableSymptoms()
        {
            var symptoms = new List<string>
            {
                "memory_loss", "confusion", "disorientation", "tremors", "stiff_muscles",
                "speech_issue", "blurred_vision", "fatigue", "frequent_urination",
                "joint_pain", "rash", "fever", "headache", "dizziness", "nausea",
                "numbness", "muscle_weakness", "speech_difficulty", "restlessness",
                "anxiety", "insomnia", "depression", "chest_pain", "shortness_breath"
            };

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Message = "Available symptoms retrieved successfully",
                Data = symptoms
            });
        }

        private (bool IsValid, List<string> Errors) ValidateRequest(SymptomRequest request)
        {
            var errors = new List<string>();

            if (request.Age < 0 || request.Age > 120)
                errors.Add("Age must be between 0 and 120");

            if (request.Weight <= 0 || request.Weight > 500)
                errors.Add("Weight must be between 0 and 500 kg");

            if (request.SystolicBP < 70 || request.SystolicBP > 250)
                errors.Add("Systolic blood pressure must be between 70 and 250 mmHg");

            if (request.DiastolicBP < 40 || request.DiastolicBP > 150)
                errors.Add("Diastolic blood pressure must be between 40 and 150 mmHg");

            if (request.HeartRate < 30 || request.HeartRate > 200)
                errors.Add("Heart rate must be between 30 and 200 bpm");

            if (request.Temperature < 35 || request.Temperature > 42)
                errors.Add("Body temperature must be between 35 and 42 degrees Celsius");

            if (request.Symptoms.Count == 0)
                errors.Add("At least 3 symptoms must be provided for accurate analysis");

            if (request.Symptoms.Count < 3)
                errors.Add("Minimum 3 symptoms required for reliable diagnosis");

            if (request.Symptoms.Count > 5)
                errors.Add("Maximum 5 symptoms can be provided");

            return (errors.Count == 0, errors);
        }
    }
}