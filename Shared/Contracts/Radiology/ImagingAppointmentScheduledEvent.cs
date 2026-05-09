using MediatR;

namespace ClinicalDentistSystem.Shared.Contracts.Radiology;

public record ImagingAppointmentScheduledEvent(
    int ImagingId,
    int PatientId,
    string Modality,
    DateTime ScheduledAt) : INotification;