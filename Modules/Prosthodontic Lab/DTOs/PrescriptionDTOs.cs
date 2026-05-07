using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class ToothArrayRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable<int> teeth)
            {
                foreach (var tooth in teeth)
                {
                    if (tooth < 11 || tooth > 48)
                    {
                        return new ValidationResult($"Tooth number {tooth} is invalid. Must be between 11 and 48.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }

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

        [ToothArrayRange]
        public List<int> ToothNumbers { get; set; } = new List<int>();

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

        [ToothArrayRange]
        public List<int> ToothNumbers { get; set; } = new List<int>();

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
        public List<int> ToothNumbers { get; set; } = new List<int>();
        public string Notes { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public DateTime DueDate { get; set; }
    }
}
