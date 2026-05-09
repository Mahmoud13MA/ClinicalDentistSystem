using Hl7.Fhir.Model;

namespace ClinicalDentistSystem.Shared.Services;

public class FhirValidationService : IFhirValidationService
{
    public OperationOutcome Validate(Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var outcome = new OperationOutcome();

        switch (resource)
        {
            case ServiceRequest serviceRequest:
                ValidateServiceRequest(serviceRequest, outcome);
                break;
            case DiagnosticReport diagnosticReport:
                ValidateDiagnosticReport(diagnosticReport, outcome);
                break;
            case DeviceRequest deviceRequest:
                ValidateDeviceRequest(deviceRequest, outcome);
                break;
            case Hl7.Fhir.Model.Task task:
                ValidateTask(task, outcome);
                break;
            default:
                break;
        }

        return outcome;
    }

    private static void ValidateServiceRequest(ServiceRequest request, OperationOutcome outcome)
    {
        if (string.IsNullOrWhiteSpace(request.Subject?.Reference))
            AddError(outcome, "ServiceRequest.Subject is required.");

        if (request.Code == null)
            AddError(outcome, "ServiceRequest.Code is required.");

        if (request.Status == null)
            AddError(outcome, "ServiceRequest.Status is required.");
    }

    private static void ValidateDiagnosticReport(DiagnosticReport report, OperationOutcome outcome)
    {
        if (string.IsNullOrWhiteSpace(report.Subject?.Reference))
            AddError(outcome, "DiagnosticReport.Subject is required.");

        if (report.Code == null)
            AddError(outcome, "DiagnosticReport.Code is required.");

        if (report.Status == null)
            AddError(outcome, "DiagnosticReport.Status is required.");
    }

    private static void ValidateDeviceRequest(DeviceRequest request, OperationOutcome outcome)
    {
        if (string.IsNullOrWhiteSpace(request.Subject?.Reference))
            AddError(outcome, "DeviceRequest.Subject is required.");

        if (request.Code == null)
            AddError(outcome, "DeviceRequest.Code is required.");

        if (request.Status == null)
            AddError(outcome, "DeviceRequest.Status is required.");
    }

    private static void ValidateTask(Hl7.Fhir.Model.Task task, OperationOutcome outcome)
    {
        if (string.IsNullOrWhiteSpace(task.For?.Reference))
            AddError(outcome, "Task.For is required.");

        if (task.Status == null)
            AddError(outcome, "Task.Status is required.");

        // ← Fix: Description was documented as required but never validated
        if (string.IsNullOrWhiteSpace(task.Description))
            AddError(outcome, "Task.Description is required.");
    }

    private static void AddError(OperationOutcome outcome, string diagnostics)
    {
        outcome.Issue.Add(new OperationOutcome.IssueComponent
        {
            Severity = OperationOutcome.IssueSeverity.Error,
            Code = OperationOutcome.IssueType.Invalid,
            Diagnostics = diagnostics
        });
    }
}