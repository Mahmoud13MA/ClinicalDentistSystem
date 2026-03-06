using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public interface IStockTransactionMappingService
    {
        StockTransactionResponse MapToResponse(Stock_Transaction transaction);
        List<StockTransactionResponse> MapToResponseList(List<Stock_Transaction> transactions);
    }
}
