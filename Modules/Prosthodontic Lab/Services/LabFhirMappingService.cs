using clinical.APIs.Modules.DentalClinic.Models;
using Hl7.Fhir.Model;

namespace clinical.APIs.Modules.ProsthodonticLab.Services;

public class LabFhirMappingService : ILabFhirMappingService
{
    public DeviceRequest MapOrderToDeviceRequest(Order order)
    {
        return new DeviceRequest
        {
            Id = order.OrderID.ToString(),
            Status = RequestStatus.Active,
            Intent = RequestIntent.Order,
            Code = new CodeableConcept { Text = string.IsNullOrWhiteSpace(order.Notes) ? "Lab Order" : order.Notes },
            Subject = new ResourceReference($"Patient/{order.PatientId}"),
            Occurrence = new FhirDateTime(order.RequiredDate == default ? order.OrderDate : order.RequiredDate),
            Requester = new ResourceReference($"Practitioner/{order.DentistId}")
        };
    }

    public DeviceRequest MapServiceRequestToDeviceRequest(ServiceRequest labOrderRequest)
    {
        ArgumentNullException.ThrowIfNull(labOrderRequest);

        return new DeviceRequest
        {
            Id = labOrderRequest.Id,
            Status = MapStatus(labOrderRequest.Status),
            Intent = RequestIntent.Order,
            Code = labOrderRequest.Code,
            Subject = labOrderRequest.Subject,
            Occurrence = labOrderRequest.Occurrence,
            Requester = labOrderRequest.Requester  // ← fix: belongs here, not above
        };
    }

    public Hl7.Fhir.Model.Task MapPrescriptionToTask(Prescription prescription)
    {
        return new Hl7.Fhir.Model.Task
        {
            Id = prescription.PrescriptionID.ToString(),
            Status = Hl7.Fhir.Model.Task.TaskStatus.InProgress,
            Description = string.IsNullOrWhiteSpace(prescription.Notes) ? "Lab Task" : prescription.Notes,
            For = new ResourceReference($"Patient/{prescription.PatientId}"),
            ExecutionPeriod = new Period
            {
                End = (prescription.DueDate == default ? DateTime.UtcNow : prescription.DueDate).ToString("O")
            }
        };
    }

    private static RequestStatus? MapStatus(RequestStatus? status)
    {
        return status ?? RequestStatus.Active;
    }
}