using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kori.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatRole
{
    User,
    Assistant
}

public sealed class ChatTurn
{
    [JsonPropertyName("role")]
    public ChatRole Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public sealed class Conversation
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatTurn> Messages { get; set; } = new();
}

