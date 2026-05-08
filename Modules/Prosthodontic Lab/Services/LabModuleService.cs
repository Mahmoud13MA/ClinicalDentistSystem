using ClinicalDentistSystem.Shared.Contracts.Lab;
using clinical.APIs.Shared.Data;
using Hl7.Fhir.Model;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.ProsthodonticLab.Services;

public class LabModuleService(AppDbContext context, ILabFhirMappingService mappingService) : ILabModule
{
    public async Task<Resource?> GetLabOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return null;
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.OrderID == parsedOrderId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        return mappingService.MapOrderToDeviceRequest(order);
    }
}
