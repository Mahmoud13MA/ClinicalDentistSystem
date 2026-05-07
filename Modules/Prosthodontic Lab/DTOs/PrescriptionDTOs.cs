
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class PrescriptionCreateRequest
    {
        public int LabTechnicianID { get; set; }
        public int OrderID { get; set; }
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;

        [Range(11, 48, ErrorMessage = "Tooth number must be between 11 and 48.")]
        public int ToothNumber { get; set; }

        public string Notes { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class PrescriptionUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;

        [Range(11, 48, ErrorMessage = "Tooth number must be between 11 and 48.")]
        public int ToothNumber { get; set; }

        public string Notes { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class PrescriptionPatchStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PrescriptionResponse
    {
        public int PrescriptionID { get; set; }
        public int LabTechnicianID { get; set; }
        public int OrderID { get; set; }
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;
        public int ToothNumber { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }
}
