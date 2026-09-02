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
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public sealed class Conversation
{
    public Guid Id { get; set; }
    public List<ChatTurn> Messages { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
}

