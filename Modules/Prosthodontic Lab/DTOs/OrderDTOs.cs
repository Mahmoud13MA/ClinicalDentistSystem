using System;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class OrderCreateRequest
    {
        public int LabTechnicianID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
    }

    public class OrderUpdateRequest
    {
        public string ShippingMethod { get; set; } = string.Empty;
    }

    public class OrderResponse
    {
        public int OrderID { get; set; }
        public int LabTechnicianID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
