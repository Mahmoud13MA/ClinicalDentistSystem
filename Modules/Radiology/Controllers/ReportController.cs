using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize(Policy = "RadiologistOrAdmin")]
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class ReportController(AppDbContext context) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await context.Reports
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

   
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            var report = await context.Reports
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

        [HttpGet("byappointment/{imagingId}")]
        public async Task<IActionResult> GetReportsByImagingAppointment(int imagingId)
        {
            var reports = await context.Reports
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
        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetReportsByPatient(int patientId)
        {
            var reports = await context.Reports
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

        
        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetReportsByRadiologist(int radiologistId)
        {
            var reports = await context.Reports
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

       
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest request)
        {
          

            var imaging = await context.ImagingAppointments.FindAsync(request.ImagingID);
            if (imaging == null)
            {
                return BadRequest(new { error = "Invalid imaging appointment ID" });
            }

            var patientExists = await context.RadiologyPatients
                .AnyAsync(p=>p.PatientID==request.PatientID);
            if (!patientExists)
            {
                return BadRequest(new { error = "Invalid patient ID" });
            }

            var radiologist = await context.Radiologists
                .FindAsync(request.RadiologistID);
            if (radiologist== null)
            {
                return BadRequest(new { error = "Invalid radiologist ID" });
            }

            var report = new Report
            {
                Findings = request.Findings,
                Diagnosis = request.Diagnosis,
                ImagingID = request.ImagingID,
                PatientID = request.PatientID,
                RadiologistID = request.RadiologistID,

                // for the mapper service

                ImagingAppointment = imaging,
                Radiologist=radiologist



            };

            context.Reports.Add(report);
            await context.SaveChangesAsync();


            var response = MapToReportResponse(report);
            return CreatedAtAction(nameof(GetReportById), new { reportId = report.ReportID }, response);
        }

        /// <summary>
        /// Update an existing report
        /// </summary>
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] ReportUpdateRequest request)
        {
         

            if (reportId != request.ReportID)
            {
                return BadRequest(new { error = "Report ID mismatch between URL and body" });
            }

            var report = await context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return NotFound(new { error = "Report not found", reportId = reportId });
            }

            // Validate that imaging appointment exists
            var imaging = await context.ImagingAppointments
                .FindAsync(request.ImagingID);
            if (imaging == null)
            {
                return BadRequest(new { error = "Invalid imaging appointment ID" });
            }

            // Validate that patient exists
            var patientExists = await context.RadiologyPatients
                .AnyAsync(p => p.PatientID == request.PatientID);
            if (!patientExists)
            {
                return BadRequest(new { error = "Invalid patient ID" });
            }

            // Validate that radiologist exists
            var radiologist = await context.Radiologists
                .FindAsync(request.RadiologistID);
            if (radiologist == null)
            {
                return BadRequest(new { error = "Invalid radiologist ID" });
            }

            // Update report
            report.Findings = request.Findings;
            report.Diagnosis = request.Diagnosis;
            report.ImagingID = request.ImagingID;
            report.PatientID = request.PatientID;
            report.RadiologistID = request.RadiologistID;

            // for the mapping service 
            report.ImagingAppointment = imaging;
            report.Radiologist = radiologist;

            

            await context.SaveChangesAsync();

          

            var response = MapToReportResponse(report);
            return Ok(new { message = "Report updated successfully", data = response });
        }

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
