namespace clinical.APIs.DTOs
{
    public class AutoCompleteRequest
    {
        public string PartialText { get; set; } = string.Empty;
        public string? Context { get; set; }
    }
}
