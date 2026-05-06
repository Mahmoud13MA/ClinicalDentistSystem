using System;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class PrescriptionCreateRequest
    {
        public int LabTechnicianID { get; set; }
        public int OrderID { get; set; }
        public string Shade { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class PrescriptionUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PrescriptionResponse
    {
        public int PrescriptionID { get; set; }
        public int LabTechnicianID { get; set; }
        public int OrderID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }
}
