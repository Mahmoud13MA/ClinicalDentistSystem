using Hl7.Fhir.Model;
using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Diagnostics;

public record RadiologyReportCompletedEvent(DiagnosticReport Report) : INotification;
