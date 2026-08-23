using System;
using System.Text.Json.Serialization;

namespace Kori.Models;

public sealed class Goal
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status {get; set;}
    [JsonPropertyName("start_at")] public DateTimeOffset Start { get; set; }
    [JsonPropertyName("end_at")] public DateTimeOffset End { get; set; }
    
}