using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kori.Models;

public sealed partial class Connector : ObservableObject
{
    public string Provider {get; set;} = string.Empty;
    public string Category {get; set;} = string.Empty;
    [ObservableProperty] 
    [property: JsonPropertyName("active")] private bool _active;
    public string Icon {get; set;} = string.Empty;
}

public sealed class ConnectorGroup
{
    public string Provider { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ObservableCollection<Connector> Entries { get; set; } = new();
}