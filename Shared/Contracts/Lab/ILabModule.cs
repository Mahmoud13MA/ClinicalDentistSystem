using Hl7.Fhir.Model;

namespace ClinicalDentistSystem.Shared.Contracts.Lab;

public interface ILabModule
{
    Task<Resource?> GetLabOrderAsync(string orderId, CancellationToken cancellationToken = default);
}
