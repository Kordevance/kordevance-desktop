using System;
using System.Text.Json.Serialization;

namespace Kori.Models;

public sealed class Device
{
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("is_owner")] public bool IsOwner { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}