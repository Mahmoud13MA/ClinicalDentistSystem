
namespace ClinicalDentistSystem.Shared.Contracts.Ownership;

public enum ModuleName
{
    DentalClinic,
    Radiology
}

public enum DataDomain
{
    PatientDemographics,
    ClinicalTimelines,
    DiagnosticImages,
    DiagnosticReports
}

public static class ModuleOwnership
{
    private static readonly IReadOnlyDictionary<ModuleName, IReadOnlyCollection<DataDomain>> OwnershipMap =
        new Dictionary<ModuleName, IReadOnlyCollection<DataDomain>>
        {
            [ModuleName.DentalClinic] = new[]
            {
                DataDomain.PatientDemographics,
                DataDomain.ClinicalTimelines
            },
            [ModuleName.Radiology] = new[]
            {
                DataDomain.DiagnosticImages,
                DataDomain.DiagnosticReports
            }
        };

    public static bool IsOwner(ModuleName module, DataDomain domain)
        => OwnershipMap.TryGetValue(module, out var domains) && domains.Contains(domain);

    public static IReadOnlyCollection<DataDomain> GetDomains(ModuleName module)
        => OwnershipMap.TryGetValue(module, out var domains) ? domains : System.Array.Empty<DataDomain>();
}
