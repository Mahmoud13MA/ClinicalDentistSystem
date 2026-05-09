using ClinicalDentistSystem.Shared.Contracts.Radiology;
using clinical.APIs.Shared.Data;
using Hl7.Fhir.Model;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace clinical.APIs.Modules.Radiology.Services;

public class RadiologyModuleService(AppDbContext context) : IRadiologyModule
{
    public async Task<DiagnosticReport?> GetReportAsync(string reportId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(reportId, out var parsedReportId))
        {
            return null;
        }

        var report = await context.Reports
            .Include(r => r.ImagingAppointment)
            .FirstOrDefaultAsync(r => r.ReportID == parsedReportId, cancellationToken);

        if (report == null)
        {
            return null;
        }

        var imaging = report.ImagingAppointment;
        var effectiveDate = imaging?.Datetime ?? DateTime.UtcNow;
        var reportDate = imaging?.Datetime ?? effectiveDate;

        return new DiagnosticReport
        {
            Id = report.ReportID.ToString(),
            Status = DiagnosticReport.DiagnosticReportStatus.Final,
            Code = new CodeableConcept { Text = imaging?.Type ?? "X-Ray" },
            Effective = new FhirDateTime(reportDate),
            Subject = new ResourceReference($"Patient/{report.PatientID}"),
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
