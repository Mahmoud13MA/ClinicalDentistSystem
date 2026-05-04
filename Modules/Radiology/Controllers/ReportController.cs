using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class ReportController(AppDbContext context , IMapper mapper) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await context.Reports
                 .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                 .ToListAsync();

            if (!reports.Any())
                return NotFound(new { error = "No reports found." });

            return Ok(reports);
        }

   
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            var report = await context.Reports
              .Where(r => r.ReportID == reportId)
              .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
              .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { error = "Report not found", reportId = reportId });

            return Ok(report);
        }

        [HttpGet("byappointment/{imagingId}")]
        public async Task<IActionResult> GetReportsByImagingAppointment(int imagingId)
        {
            var reports = await context.Reports
              .Where(r => r.ImagingID == imagingId)
              .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
              .ToListAsync();

            if (!reports.Any())
                return NotFound(new { error = "No reports found for this imaging appointment", imagingId = imagingId });

            return Ok(reports);
        }
        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetReportsByPatient(int patientId)
        {
            var reports = await context.Reports
             .Where(r => r.PatientID == patientId)
             .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
             .ToListAsync();

            if (!reports.Any())
                return NotFound(new { error = "No reports found for this patient", patientId = patientId });

            return Ok(reports);
        }

        
        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetReportsByRadiologist(int radiologistId)
        {
            var reports = await context.Reports
                .Where(r => r.RadiologistID == radiologistId)
                .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            if (!reports.Any())
            {
                return NotFound(new { error = "No reports found for this radiologist", radiologistId = radiologistId });
            }

          
            return Ok(reports);
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest request)
        {

            var imaging = await context.ImagingAppointments.FindAsync(request.ImagingID);
            if (imaging == null)
                return BadRequest(new { error = "Invalid imaging appointment ID" });

            var patientExists = await context.RadiologyPatients.AnyAsync(p => p.PatientID == request.PatientID);
            if (!patientExists)
                return BadRequest(new { error = "Invalid patient ID" });

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
                return BadRequest(new { error = "Invalid radiologist ID" });

            var report = mapper.Map<Report>(request);
            report.ImagingAppointment = imaging;
            report.Radiologist = radiologist;

            context.Reports.Add(report);
            await context.SaveChangesAsync();

            var response = mapper.Map<ReportResponse>(report);
            return CreatedAtAction(nameof(GetReportById), new { reportId = report.ReportID }, response);
        }

      
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] ReportUpdateRequest request)
        {


            if (reportId != request.ReportID)
                return BadRequest(new { error = "Report ID mismatch between URL and body" });

            var report = await context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound(new { error = "Report not found", reportId = reportId });

            // Validate and load navigation properties
            var imaging = await context.ImagingAppointments.FindAsync(request.ImagingID);
            if (imaging == null)
                return BadRequest(new { error = "Invalid imaging appointment ID" });

            var patientExists = await context.RadiologyPatients.AnyAsync(p => p.PatientID == request.PatientID);
            if (!patientExists)
                return BadRequest(new { error = "Invalid patient ID" });

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
                return BadRequest(new { error = "Invalid radiologist ID" });

            mapper.Map(request, report);
            // for the mapper
            report.ImagingAppointment = imaging;
            report.Radiologist = radiologist;

            await context.SaveChangesAsync();

            var response = mapper.Map<ReportResponse>(report);
            return Ok(new { message = "Report updated successfully", data = response });
        }

       
    }
}
