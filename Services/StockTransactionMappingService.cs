using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public class StockTransactionMappingService : IStockTransactionMappingService
    {
        public StockTransactionResponse MapToResponse(Stock_Transaction transaction)
        {
            if (transaction == null)
                return null;

            return new StockTransactionResponse
            {
                T_ID = transaction.T_ID,
                Date = transaction.Date,
                Time = transaction.Time,
                Quantity = transaction.Quantity,
                Doctor_ID = transaction.Doctor_ID,
                Supply_ID = transaction.Supply_ID,
                Doctor = transaction.Doctor != null ? new DoctorBasicInfo
                {
                    ID = transaction.Doctor.ID,
                    Name = transaction.Doctor.Name,
                    Phone = transaction.Doctor.Phone,
                    Email = transaction.Doctor.Email
                } : null,
                Supply = transaction.Supply != null ? new SupplyBasicInfo
                {
                    Supply_ID = transaction.Supply.Supply_ID,
                    Supply_Name = transaction.Supply.Supply_Name,
                    Category = transaction.Supply.Category,
                    Unit = transaction.Supply.Unit,
                    Quantity = transaction.Supply.Quantity
                } : null
            };
        }

        public List<StockTransactionResponse> MapToResponseList(List<Stock_Transaction> transactions)
        {
            if (transactions == null)
                return new List<StockTransactionResponse>();

            return transactions.Select(t => MapToResponse(t)).ToList();
        }
    }
}
