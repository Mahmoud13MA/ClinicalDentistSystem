namespace clinical.APIs.Modules.ProsthodonticLab.DTOs;

public class LabOrderRetryPayload
{
    public string? FhirId { get; set; }
    public string? FhirResource { get; set; }
}