using System.Text.Json.Serialization;

namespace Kori.Models;

public sealed class Chat
{
    [JsonPropertyName("conversation_id")] public string? ConversationId { get; set; }
    public string? Message { get; set; }
    public string? Reply { get; set; }
}
