using System.Text.Json.Serialization;

namespace Kori.Models;

public sealed class Provider
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public string ApiKey {get; set;} = string.Empty;
    public string? EndpointUrl {get; set;}
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModelAssignmentType
{
    Primary,
    Triage,
    Discovery
}

public sealed class ModelAssignment
{
    public ModelAssignmentType Role {get; set;}
    [JsonPropertyName("provider_id")] public string ProviderId {get; set;} = string.Empty;
    [JsonPropertyName("model_id")] public string ModelId {get; set;} = string.Empty;
}