using Microsoft.AspNetCore.Mvc;
using MedicalDiagnosisAPI.Models;
using MedicalDiagnosisAPI.Services;

namespace MedicalDiagnosisAPI.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class SymptomsController : ControllerBase
    {
        private readonly IFrontendMedicalService _frontendService;
        private readonly IQuestionService _questionService;
        private readonly ILogger<SymptomsController> _logger;

        public SymptomsController(IFrontendMedicalService frontendService, IQuestionService questionService, ILogger<SymptomsController> logger)
        {
            _frontendService = frontendService;
            _questionService = questionService;
            _logger = logger;
        }

        /// <summary>
        /// Analyzes symptoms and provides diagnosis suggestions - optimized for frontend
        /// </summary>
        /// <param name="request">Simple symptom input</param>
        /// <returns>Enhanced diagnosis response with recommendations</returns>
        [HttpPost("analyze")]
        [ProducesResponseType(typeof(ApiResponse<EnhancedDiagnosisResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<EnhancedDiagnosisResponse>>> AnalyzeSymptoms(
            [FromBody] SimpleSymptomRequest request)
        {
            try
            {
                var validationResult = ValidateSimpleRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = validationResult.Errors
                    });
                }

                var analysis = await _frontendService.AnalyzeSymptomsAsync(request);

                return Ok(new ApiResponse<EnhancedDiagnosisResponse>
                {
                    Success = true,
                    Message = "Symptom analysis completed successfully",
                    Data = analysis
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing symptoms");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while analyzing symptoms",
                    Errors = new List<string> { "Please try again later" }
                });
            }
        }

        /// <summary>
        /// Assesses urgency level of symptoms for triage
        /// </summary>
        /// <param name="request">Symptom input for triage</param>
        /// <returns>Triage assessment with urgency level</returns>
        [HttpPost("triage")]
        [ProducesResponseType(typeof(ApiResponse<TriageResponse>), 200)]
        public async Task<ActionResult<ApiResponse<TriageResponse>>> AssessUrgency(
            [FromBody] SimpleSymptomRequest request)
        {
            try
            {
                var triage = await _frontendService.AssessUrgencyAsync(request);

                return Ok(new ApiResponse<TriageResponse>
                {
                    Success = true,
                    Message = "Triage assessment completed",
                    Data = triage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in triage assessment");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Triage assessment failed",
                    Errors = new List<string> { "Please try again later" }
                });
            }
        }

        /// <summary>
        /// Get predefined questions for enhanced diagnosis
        /// </summary>
        [HttpGet("questions")]
        [ProducesResponseType(typeof(ApiResponse<List<PredefinedQuestion>>), 200)]
        public ActionResult<ApiResponse<List<PredefinedQuestion>>> GetPredefinedQuestions()
        {
            try
            {
                var questions = _questionService.GetPredefinedQuestions();
                
                return Ok(new ApiResponse<List<PredefinedQuestion>>
                {
                    Success = true,
                    Message = "Predefined questions retrieved successfully",
                    Data = questions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving predefined questions");
                return StatusCode(500, new ApiResponse<List<PredefinedQuestion>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving questions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets organized symptom categories for UI display
        /// </summary>
        /// <returns>Symptom categories with metadata</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<List<SymptomCategory>>), 200)]
        public async Task<ActionResult<ApiResponse<List<SymptomCategory>>>> GetSymptomCategories()
        {
            try
            {
                var categories = await _frontendService.GetSymptomCategoriesAsync();

                return Ok(new ApiResponse<List<SymptomCategory>>
                {
                    Success = true,
                    Message = "Symptom categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving symptom categories");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve symptom categories"
                });
            }
        }

        /// <summary>
        /// Finds similar symptoms for autocomplete/suggestions
        /// </summary>
        /// <param name="query">Partial symptom name</param>
        /// <returns>List of similar symptoms</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public async Task<ActionResult<ApiResponse<List<string>>>> SearchSymptoms([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Ok(new ApiResponse<List<string>>
                    {
                        Success = true,
                        Message = "Query too short",
                        Data = new List<string>()
                    });
                }

                var suggestions = await _frontendService.GetSimilarSymptomsAsync(query);

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Symptom suggestions retrieved",
                    Data = suggestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching symptoms");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Search failed"
                });
            }
        }

        /// <summary>
        /// Comprehensive health assessment with lifestyle factors
        /// </summary>
        /// <param name="request">Detailed health assessment request</param>
        /// <returns>Comprehensive diagnosis response</returns>
        [HttpPost("comprehensive")]
        [ProducesResponseType(typeof(ApiResponse<EnhancedDiagnosisResponse>), 200)]
        public async Task<ActionResult<ApiResponse<EnhancedDiagnosisResponse>>> ComprehensiveAssessment(
            [FromBody] HealthAssessmentRequest request)
        {
            try
            {
                var validationResult = ValidateHealthAssessmentRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid assessment data",
                        Errors = validationResult.Errors
                    });
                }

                var assessment = await _frontendService.ComprehensiveHealthAssessmentAsync(request);

                return Ok(new ApiResponse<EnhancedDiagnosisResponse>
                {
                    Success = true,
                    Message = "Comprehensive health assessment completed",
                    Data = assessment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in comprehensive assessment");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Assessment failed",
                    Errors = new List<string> { "Please try again later" }
                });
            }
        }

        /// <summary>
        /// Health check endpoint for frontend monitoring
        /// </summary>
        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "2.0",
                Services = new
                {
                    SymptomAnalysis = "Available",
                    TriageAssessment = "Available",
                    SymptomCategories = "Available"
                }
            });
        }

        private (bool IsValid, List<string> Errors) ValidateSimpleRequest(SimpleSymptomRequest request)
        {
            var errors = new List<string>();

            if (request.Age < 0 || request.Age > 150)
                errors.Add("Age must be between 0 and 150");

            if (request.Symptoms == null || request.Symptoms.Count == 0)
                errors.Add("At least 3 symptoms must be provided for accurate analysis");

            if (request.Symptoms.Count < 3)
                errors.Add("Minimum 3 symptoms required for reliable diagnosis. Please add more symptoms.");

            if (request.Symptoms.Count > 10)
                errors.Add("Maximum 10 symptoms allowed");

            foreach (var symptom in request.Symptoms)
            {
                if (string.IsNullOrWhiteSpace(symptom.Name))
                    errors.Add("Symptom name cannot be empty");

                if (!new[] { "mild", "moderate", "severe" }.Contains(symptom.Severity.ToLower()))
                    errors.Add($"Invalid severity level: {symptom.Severity}. Must be mild, moderate, or severe");

                if (symptom.Duration < 0 || symptom.Duration > 365)
                    errors.Add($"Symptom duration must be between 0 and 365 days");
            }

            return (errors.Count == 0, errors);
        }

        private (bool IsValid, List<string> Errors) ValidateHealthAssessmentRequest(HealthAssessmentRequest request)
        {
            var errors = new List<string>();

            if (request.Age < 0 || request.Age > 150)
                errors.Add("Age must be between 0 and 150");

            if (request.Weight.HasValue && (request.Weight < 1 || request.Weight > 500))
                errors.Add("Weight must be between 1 and 500 kg");

            if (request.Height.HasValue && (request.Height < 50 || request.Height > 250))
                errors.Add("Height must be between 50 and 250 cm");

            if (request.Symptoms == null || request.Symptoms.Count == 0)
                errors.Add("At least one symptom must be provided");

            return (errors.Count == 0, errors);
        }
    }
}