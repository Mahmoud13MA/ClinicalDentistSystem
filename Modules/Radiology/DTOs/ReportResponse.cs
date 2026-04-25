namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ReportResponse
    {
        public int ReportID { get; set; }
        public string Findings { get; set; }
        public string Diagnosis { get; set; }
        public int ImagingID { get; set; }
        public ImagingAppointmentBasicInfo ImagingAppointment { get; set; }
        public int PatientID { get; set; }
        public PatientBasicInfoRadiology Patient { get; set; }
        public int RadiologistID { get; set; }
        public RadiologistBasicInfo Radiologist { get; set; }
    }
}
