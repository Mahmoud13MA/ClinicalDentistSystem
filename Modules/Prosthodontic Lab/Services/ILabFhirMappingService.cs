using Hl7.Fhir.Model;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.ProsthodonticLab.Services;

public interface ILabFhirMappingService
{
    DeviceRequest MapOrderToDeviceRequest(Order order);
    DeviceRequest MapServiceRequestToDeviceRequest(ServiceRequest labOrderRequest);
    Hl7.Fhir.Model.Task MapPrescriptionToTask(Prescription prescription);
}
