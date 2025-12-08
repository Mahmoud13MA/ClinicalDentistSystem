using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public interface IStockTransactionMappingService
    {
        StockTransactionResponse MapToResponse(Stock_Transaction transaction);
        List<StockTransactionResponse> MapToResponseList(List<Stock_Transaction> transactions);
    }
}
