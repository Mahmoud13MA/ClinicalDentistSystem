using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly ILlamaService _llamaService;

        public AIController(ILlamaService llamaService)
        {
            _llamaService = llamaService;
        }

        /// <summary>
        /// Get auto-complete suggestions for clinical notes
        /// </summary>
        [HttpPost("autocomplete")]
        public async Task<IActionResult> GetAutoComplete([FromBody] AutoCompleteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PartialText))
            {
                return BadRequest(new { error = "Partial text is required" });
            }

            var suggestions = await _llamaService.GetAutoCompleteSuggestionsAsync(request.PartialText, request.Context ?? "");
            return Ok(new { suggestions });
        }

        /// <summary>
        /// Get dental terminology suggestions
        /// </summary>
        [HttpPost("terminology")]
        public async Task<IActionResult> GetDentalTerminology([FromBody] TerminologyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PartialTerm))
            {
                return BadRequest(new { error = "Partial term is required" });
            }

            var suggestions = await _llamaService.GetDentalTerminologySuggestionsAsync(request.PartialTerm);
            return Ok(new { suggestions });
        }

        /// <summary>
        /// Generate complete clinical notes from bullet points
        /// </summary>
        [HttpPost("generate-notes")]
        public async Task<IActionResult> GenerateClinicalNotes([FromBody] GenerateNotesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BulletPoints))
            {
                return BadRequest(new { error = "Bullet points are required" });
            }

            var notes = await _llamaService.GenerateClinicalNotesAsync(request.BulletPoints, request.PatientContext ?? "");
            return Ok(new { generatedNotes = notes });
        }

        /// <summary>
        /// Suggest treatments based on diagnosis
        /// </summary>
        [HttpPost("suggest-treatments")]
        public async Task<IActionResult> SuggestTreatments([FromBody] TreatmentSuggestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Diagnosis))
            {
                return BadRequest(new { error = "Diagnosis is required" });
            }

            var treatments = await _llamaService.SuggestTreatmentsAsync(request.Diagnosis, request.PatientHistory ?? "");
            return Ok(new { treatments });
        }

        /// <summary>
        /// Extract structured clinical data from free text
        /// </summary>
        [HttpPost("extract-clinical-data")]
        public async Task<IActionResult> ExtractClinicalData([FromBody] ExtractDataRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FreeText))
            {
                return BadRequest(new { error = "Free text is required" });
            }

            var result = await _llamaService.ExtractClinicalDataAsync(request.FreeText);
            return Ok(result);
        }

        /// <summary>
        /// Parse large doctor's text and extract ALL EHR fields automatically
        /// </summary>
        [HttpPost("parse-to-ehr")]
        public async Task<IActionResult> ParseToEHR(
            [FromBody] ParseEHRRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.LargeText))
            {
                return BadRequest(new { error = "Large text is required" });
            }

            if (request.LargeText.Length > 10000)
            {
                return BadRequest(new { error = "Text exceeds maximum allowed length of 10,000 characters" });
            }

            var result = await _llamaService.ParseToCompleteEHRAsync(request.LargeText, request.PatientContext ?? "", cancellationToken);

            var response = new ParseEHRResponse
            {
                Success = true,
                Message = "EHR fields extracted successfully",
                ExtractedFields = new EHRFieldsResponse
                {
                    Allergies = result.Allergies,
                    MedicalAlerts = result.MedicalAlerts,
                    Diagnosis = result.Diagnosis,
                    XRayFindings = result.XRayFindings,
                    PeriodontalStatus = result.PeriodontalStatus,
                    ClinicalNotes = result.ClinicalNotes,
                    Recommendations = result.Recommendations,
                    History = result.History,
                    Treatments = result.Treatments,
                    Medications = result.Medications?.Select(m => new MedicationData
                    {
                        Name = m.Name,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        Duration = m.Duration
                    }).ToList(),
                    Procedures = result.Procedures?.Select(p => new ProcedureData
                    {
                        Name = p.Name,
                        Description = p.Description,
                        Date = p.Date
                    }).ToList(),
                    AffectedTeeth = result.AffectedTeeth?.Select(t => new ToothData
                    {
                        ToothNumber = t.ToothNumber,
                        Condition = t.Condition,
                        Treatment = t.Treatment
                    }).ToList(),
                    XRays = result.XRays?.Select(x => new XRayData
                    {
                        Type = x.Type,
                        Findings = x.Findings,
                        Date = x.Date
                    }).ToList()
                }
            };

            return Ok(response);
        }
    }
}
