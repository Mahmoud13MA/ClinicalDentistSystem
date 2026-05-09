using ClinicalDentistSystem.Shared.Contracts.Patients;
using clinical.APIs.Shared.Data;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Handlers;

public class GetPatientByIdQueryHandler(AppDbContext context, IDentalClinicPatientFhirMapper mapper)
    : IRequestHandler<GetPatientByIdQuery, Patient?>  // ← nullable
{
    public async Task<Patient?> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.PatientId, out var patientId))
            return null;  // ← null, not new Patient()

        var patient = await context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Patient_ID == patientId, cancellationToken);

        return patient is null ? null : mapper.MapToFhirPatient(patient);
    }
}