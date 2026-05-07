using System;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class OrderCreateRequest
    {
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public int LabTechnicianID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class OrderUpdateRequest
    {
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public int LabTechnicianID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class OrderPatchStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class OrderResponse
    {
        public int OrderID { get; set; }
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public int LabTechnicianID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
