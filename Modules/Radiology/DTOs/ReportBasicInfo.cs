namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ReportBasicInfo
    {
        public int ReportID { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
    }
}
