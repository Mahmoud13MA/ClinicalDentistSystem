using clinical.APIs.Services;
using clinical.APIs.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinical.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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

            try
            {
                var suggestions = await _llamaService.GetAutoCompleteSuggestionsAsync(
                    request.PartialText, 
                    request.Context ?? ""
                );
                
                return Ok(new { suggestions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Auto-complete failed", message = ex.Message });
            }
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

            try
            {
                var suggestions = await _llamaService.GetDentalTerminologySuggestionsAsync(request.PartialTerm);
                return Ok(new { suggestions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Terminology lookup failed", message = ex.Message });
            }
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

            try
            {
                var notes = await _llamaService.GenerateClinicalNotesAsync(
                    request.BulletPoints, 
                    request.PatientContext ?? ""
                );
                
                return Ok(new { generatedNotes = notes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Note generation failed", message = ex.Message });
            }
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

            try
            {
                var treatments = await _llamaService.SuggestTreatmentsAsync(
                    request.Diagnosis, 
                    request.PatientHistory ?? ""
                );
                
                return Ok(new { treatments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Treatment suggestion failed", message = ex.Message });
            }
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

            try
            {
                var result = await _llamaService.ExtractClinicalDataAsync(request.FreeText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Data extraction failed", message = ex.Message });
            }
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

            try
            {
                var result = await _llamaService.ParseToCompleteEHRAsync(
                    request.LargeText, 
                    request.PatientContext ?? "",
                    cancellationToken
                );

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
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Request timeout", message = "EHR parsing took too long and was cancelled" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "EHR parsing failed", message = ex.Message });
            }
        }
    }
}
