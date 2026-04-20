namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class EHRChangeLogResponse
    {
        public int ChangeLog_ID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public int ChangedByDoctorId { get; set; }
        public string ChangedByDoctorName { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public int EHR_ID { get; set; }
    }
}
