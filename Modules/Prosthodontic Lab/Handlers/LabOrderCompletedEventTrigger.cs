using MediatR;

namespace clinical.APIs.Modules.ProsthodonticLab.Handlers;

public record LabOrderCompletedEventTrigger(int OrderId) : INotification;
