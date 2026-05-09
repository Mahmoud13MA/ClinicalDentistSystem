namespace clinical.APIs.Modules.DentalClinic.DTOs
{

    public record CreateLabOrderDto(
    int PatientId,
    string ProcedureDescription
);


}
