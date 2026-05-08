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
            Occurrence = new FhirDateTime(order.RequiredDate == default ? order.OrderDate : order.RequiredDate)
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
}
