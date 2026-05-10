using AutoMapper;
using AutoMapper.QueryableExtensions;
using clinical.APIs.Modules.Radiology.DTOs;
using clinical.APIs.Shared.Data;
using ClinicalDentistSystem.Shared.Contracts.Diagnostics;
using Hl7.Fhir.Model;
using ClinicalDentistSystem.Shared.Services;
using MediatR;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.Controllers
{
    [Authorize(Policy = "RadiologistOrAdmin")]
    [ApiController]
    [Route("api/v1/radiology/[controller]")]
    public class ReportController(
        AppDbContext context,
        IMapper mapper,
        IMediator mediator,
        IFhirValidationService validationService,
        ILogger<ReportController> logger) : ControllerBase  // ← added logger
    {
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await context.Reports
                .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix #4: empty collection is Ok([]), not NotFound
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
                return NotFound(new { error = "Report not found", reportId });

            return Ok(report);
        }

        [HttpGet("byappointment/{imagingId}")]
        public async Task<IActionResult> GetReportsByImagingAppointment(int imagingId)
        {
            var reports = await context.Reports
                .Where(r => r.ImagingID == imagingId)
                .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix #4: empty collection is Ok([]), not NotFound
            return Ok(reports);
        }

        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetReportsByPatient(int patientId)
        {
            var reports = await context.Reports
                .Where(r => r.PatientID == patientId)
                .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix #4: empty collection is Ok([]), not NotFound
            return Ok(reports);
        }

        [HttpGet("byradiologist/{radiologistId}")]
        public async Task<IActionResult> GetReportsByRadiologist(int radiologistId)
        {
            var reports = await context.Reports
                .Where(r => r.RadiologistID == radiologistId)
                .ProjectTo<ReportResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            // ← Fix #4: empty collection is Ok([]), not NotFound
            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest request)
        {
            // Doctor's imaging appointment request must exist — enforces proper clinical flow
            var imaging = await context.ImagingAppointments.FindAsync(request.ImagingID);
            if (imaging == null)
                return BadRequest(new { error = "Invalid imaging appointment ID — a doctor must first create a radiology service request." });

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

            var diagnosticReport = BuildDiagnosticReport(report, imaging);
            var outcome = validationService.Validate(diagnosticReport);

            // ← Fix #2: log validation errors clearly instead of silent skip
            if (HasErrors(outcome))
            {
                var errors = string.Join(", ", outcome.Issue
                    .Where(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal)
                    .Select(i => i.Diagnostics));
                logger.LogWarning("FHIR validation failed for DiagnosticReport {Id}: {Errors}", report.ReportID, errors);
            }
            else
            {
                await mediator.Publish(new RadiologyReportCompletedEvent(diagnosticReport), HttpContext.RequestAborted);
            }

            return CreatedAtAction(nameof(GetReportById), new { reportId = report.ReportID }, mapper.Map<ReportResponse>(report));
        }

        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] ReportUpdateRequest request)
        {
            if (reportId != request.ReportID)
                return BadRequest(new { error = "Report ID mismatch between URL and body" });

            var report = await context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound(new { error = "Report not found", reportId });

            var imaging = await context.ImagingAppointments.FindAsync(request.ImagingID);
            if (imaging == null)
                return BadRequest(new { error = "Invalid imaging appointment ID" });

            var patientExists = await context.RadiologyPatients.AnyAsync(p => p.PatientID == request.PatientID);
            if (!patientExists)
                return BadRequest(new { error = "Invalid patient ID" });

            var radiologist = await context.Radiologists.FindAsync(request.RadiologistID);
            if (radiologist == null)
                return BadRequest(new { error = "Invalid radiologist ID" });

            // ← Fix #3: capture previous findings to detect meaningful change
            var previousFindings = report.Findings;

            mapper.Map(request, report);
            report.ImagingAppointment = imaging;
            report.Radiologist = radiologist;
            await context.SaveChangesAsync();

            // Only re-publish if findings actually changed — prevents duplicate metadata in DentalClinic
            if (!string.Equals(previousFindings, report.Findings, StringComparison.Ordinal))
            {
                var diagnosticReport = BuildDiagnosticReport(report, imaging);
                var outcome = validationService.Validate(diagnosticReport);

                if (HasErrors(outcome))
                {
                    var errors = string.Join(", ", outcome.Issue
                        .Where(i => i.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal)
                        .Select(i => i.Diagnostics));
                    logger.LogWarning("FHIR validation failed for updated DiagnosticReport {Id}: {Errors}", report.ReportID, errors);
                }
                else
                {
                    await mediator.Publish(new RadiologyReportCompletedEvent(diagnosticReport), HttpContext.RequestAborted);
                }
            }

            return Ok(new { message = "Report updated successfully", data = mapper.Map<ReportResponse>(report) });
        }

        private static bool HasErrors(OperationOutcome outcome)
            => outcome.Issue.Any(issue => issue.Severity is OperationOutcome.IssueSeverity.Error or OperationOutcome.IssueSeverity.Fatal);

        private static DiagnosticReport BuildDiagnosticReport(Report report, ImagingAppointment imaging)
        {
            var effectiveDate = imaging.Datetime == default ? DateTime.UtcNow : imaging.Datetime;
            var reportDate = report.ImagingAppointment?.Datetime ?? effectiveDate;

            return new DiagnosticReport
            {
                Id = report.ReportID.ToString(),
                Status = DiagnosticReport.DiagnosticReportStatus.Final,
                Code = new CodeableConcept { Text = imaging.Type },
                Effective = new FhirDateTime(reportDate),
                Subject = new ResourceReference($"Patient/{report.PatientID}"),
                // ← Fix #1: Performer added — was missing, radiologist reference lost
                Performer = new List<ResourceReference>
                {
                    new ResourceReference($"Practitioner/{report.RadiologistID}")
                },
                Conclusion = report.Findings,
                PresentedForm = new List<Attachment>
                {
                    new()
                    {
                        ContentType = "text/plain",
                        Data = Encoding.UTF8.GetBytes(report.Diagnosis)
                    }
                }
            };
        }
    }
}