using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all radiology reports
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var reports = await _context.Reports
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .ToListAsync();

                if (reports == null || reports.Count == 0)
                {
                    return NotFound(new { error = "No reports found." });
                }

                var response = reports.Select(r => MapToReportResponse(r)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific report by ID
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            try
            {
                var report = await _context.Reports
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .FirstOrDefaultAsync(r => r.ReportID == reportId);

                if (report == null)
                {
                    return NotFound(new { error = "Report not found", reportId = reportId });
                }

                var response = MapToReportResponse(report);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get reports by imaging appointment
        /// </summary>
        [HttpGet("byappointment/{imagingId}")]
        public async Task<IActionResult> GetReportsByImagingAppointment(int imagingId)
        {
            try
            {
                var reports = await _context.Reports
                    .Where(r => r.ImagingID == imagingId)
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .ToListAsync();

                if (reports == null || reports.Count == 0)
                {
                    return NotFound(new { error = "No reports found for this imaging appointment", imagingId = imagingId });
                }

                var response = reports.Select(r => MapToReportResponse(r)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get reports by patient
        /// </summary>
        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetReportsByPatient(int patientId)
        {
            try
            {
                var reports = await _context.Reports
                    .Where(r => r.PatientID == patientId)
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .ToListAsync();

                if (reports == null || reports.Count == 0)
                {
                    return NotFound(new { error = "No reports found for this patient", patientId = patientId });
                }

                var response = reports.Select(r => MapToReportResponse(r)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get reports by radiologist
        /// </summary>
        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetReportsByRadiologist(int radiologistId)
        {
            try
            {
                var reports = await _context.Reports
                    .Where(r => r.RadiologistID == radiologistId)
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .ToListAsync();

                if (reports == null || reports.Count == 0)
                {
                    return NotFound(new { error = "No reports found for this radiologist", radiologistId = radiologistId });
                }

                var response = reports.Select(r => MapToReportResponse(r)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new report
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Report data is required" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { error = "Validation failed", details = errors });
            }

            try
            {
                // Validate that imaging appointment exists
                var imagingExists = await _context.ImagingAppointments
                    .AnyAsync(ia => ia.ImagingID == request.ImagingID);
                if (!imagingExists)
                {
                    return BadRequest(new { error = "Invalid imaging appointment ID" });
                }

                // Validate that patient exists
                var patientExists = await _context.RadiologyPatients
                    .AnyAsync(p => p.PatientID == request.PatientID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid patient ID" });
                }

                // Validate that radiologist exists
                var radiologistExists = await _context.Radiologists
                    .AnyAsync(r => r.RadiologistID == request.RadiologistID);
                if (!radiologistExists)
                {
                    return BadRequest(new { error = "Invalid radiologist ID" });
                }

                var report = new Report
                {
                    Findings = request.Findings,
                    Diagnosis = request.Diagnosis,
                    ImagingID = request.ImagingID,
                    PatientID = request.PatientID,
                    RadiologistID = request.RadiologistID
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // Fetch the created report with related data
                var createdReport = await _context.Reports
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .FirstOrDefaultAsync(r => r.ReportID == report.ReportID);

                var response = MapToReportResponse(createdReport);
                return CreatedAtAction(nameof(GetReportById), new { reportId = report.ReportID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing report
        /// </summary>
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] ReportUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Report data is required" });
            }

            if (reportId != request.ReportID)
            {
                return BadRequest(new { error = "Report ID mismatch between URL and body" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { error = "Validation failed", details = errors });
            }

            try
            {
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                {
                    return NotFound(new { error = "Report not found", reportId = reportId });
                }

                // Validate that imaging appointment exists
                var imagingExists = await _context.ImagingAppointments
                    .AnyAsync(ia => ia.ImagingID == request.ImagingID);
                if (!imagingExists)
                {
                    return BadRequest(new { error = "Invalid imaging appointment ID" });
                }

                // Validate that patient exists
                var patientExists = await _context.RadiologyPatients
                    .AnyAsync(p => p.PatientID == request.PatientID);
                if (!patientExists)
                {
                    return BadRequest(new { error = "Invalid patient ID" });
                }

                // Validate that radiologist exists
                var radiologistExists = await _context.Radiologists
                    .AnyAsync(r => r.RadiologistID == request.RadiologistID);
                if (!radiologistExists)
                {
                    return BadRequest(new { error = "Invalid radiologist ID" });
                }

                // Update report
                report.Findings = request.Findings;
                report.Diagnosis = request.Diagnosis;
                report.ImagingID = request.ImagingID;
                report.PatientID = request.PatientID;
                report.RadiologistID = request.RadiologistID;

                _context.Reports.Update(report);
                await _context.SaveChangesAsync();

                // Fetch updated report with related data
                var updatedReport = await _context.Reports
                    .Include(r => r.ImagingAppointment)
                    .Include(r => r.Radiologist)
                    .FirstOrDefaultAsync(r => r.ReportID == reportId);

                var response = MapToReportResponse(updatedReport);
                return Ok(new { message = "Report updated successfully", data = response });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Reports.AnyAsync(r => r.ReportID == reportId))
                {
                    return NotFound(new { error = "Report not found", reportId = reportId });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Map Report model to ReportResponse DTO
        /// </summary>
        private ReportResponse MapToReportResponse(Report report)
        {
            return new ReportResponse
            {
                ReportID = report.ReportID,
                Findings = report.Findings,
                Diagnosis = report.Diagnosis,
                ImagingID = report.ImagingID,
                ImagingAppointment = report.ImagingAppointment != null ? new ImagingAppointmentBasicInfo
                {
                    ImagingID = report.ImagingAppointment.ImagingID,
                    Datetime = report.ImagingAppointment.Datetime,
                    Type = report.ImagingAppointment.Type
                } : null,
                PatientID = report.PatientID,
                RadiologistID = report.RadiologistID,
                Radiologist = report.Radiologist != null ? new RadiologistBasicInfo
                {
                    RadiologistID = report.Radiologist.RadiologistID,
                    Name = report.Radiologist.Name,
                    Phone = report.Radiologist.Phone,
                    Email = report.Radiologist.Email,
                    Specialty = report.Radiologist.Specialty
                } : null
            };
        }
    }
}
