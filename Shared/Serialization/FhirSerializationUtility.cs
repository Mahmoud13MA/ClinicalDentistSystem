using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
namespace ClinicalDentistSystem.Shared.Serialization;

/// <summary>
/// Centralized utility for securely serializing and deserializing FHIR resources
/// to and from JSON, to be used when passing messages across an event bus or local queue.
/// </summary>
public class FhirSerializationUtility(FhirJsonSerializer? serializer = null, FhirJsonDeserializer? deserializer = null)
{
    // The official FHIR Serializer and Parser are thread-safe and should be reused.
    private readonly FhirJsonSerializer _serializer = serializer ?? new FhirJsonSerializer();

    private readonly FhirJsonDeserializer _deserializer = deserializer ?? FhirJsonDeserializer.STRICT;

    /// <summary>
    /// Serializes any standard FHIR Resource into its correct JSON representation.
    /// </summary>
    /// <param name="resource">The FHIR resource (e.g., Patient, ServiceRequest)</param>
    /// <returns>A FHIR-compliant JSON string</returns>
    public string SerializeToJson(Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return _serializer.SerializeToString(resource);
    }

    /// <summary>
    /// Deserializes a FHIR-compliant JSON string back into a heavily-typed FHIR Resource.
    /// </summary>
    /// <typeparam name="T">The expected FHIR Resource type</typeparam>
    /// <param name="json">The JSON string</param>
    /// <returns>The deserialized FHIR object</returns>
    public T DeserializeFromJson<T>(string json) where T : Resource
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));

        return _deserializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Deserializes a FHIR-compliant JSON string into a generic Resource type
    /// when the exact specific type is unknown prior to parsing.
    /// </summary>
    /// <param name="json">The JSON string</param>
    /// <returns>The base Resource</returns>
    public Resource DeserializeResourceFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));

        return _deserializer.Deserialize<Resource>(json);
    }
}
