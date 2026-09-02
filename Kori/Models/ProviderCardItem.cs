using CommunityToolkit.Mvvm.ComponentModel;

namespace Kori.Models;

public sealed partial class ProviderCardItem : ObservableObject
{
    public AuthorizedLlmProvider Kind { get; }
    public string Name { get; }
    public string DisplayName { get; }
    public string IconUrl { get; }

    [ObservableProperty]
    private Provider? _configured;

    [ObservableProperty]
    private bool _isAdding;

    [ObservableProperty]
    private string _apiKeyInput = string.Empty;

    [ObservableProperty]
    private string _endpointInput = string.Empty;

    public bool IsConfigured => Configured is not null;

    public ProviderCardItem(AuthorizedLlmProvider kind, string name, string displayName, string iconUrl)
    {
        Kind = kind;
        Name = name;
        DisplayName = displayName;
        IconUrl = iconUrl;
    }

    partial void OnConfiguredChanged(Provider? value)
    {
        OnPropertyChanged(nameof(IsConfigured));
    }
}
