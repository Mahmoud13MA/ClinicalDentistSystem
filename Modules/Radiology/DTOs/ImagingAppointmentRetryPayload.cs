namespace clinical.APIs.Modules.Radiology.DTOs;

public class ImagingAppointmentRetryPayload
{
    public int PatientID { get; set; }
    public string? Type { get; set; }
    public DateTime Datetime { get; set; }
}