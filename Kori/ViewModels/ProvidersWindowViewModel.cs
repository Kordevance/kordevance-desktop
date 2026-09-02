using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;

namespace Kori.ViewModels;

public partial class ProvidersWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INotificationService _notificationService;

    public ObservableCollection<ProviderCardItem> KnownProviders { get; } = new();

    public ObservableCollection<Provider> OtherProviders { get; } = new();

    public ObservableCollection<Models.ModelAssignmentSlot> ModelAssignments { get; } = new();

    [ObservableProperty]
    private bool _isAddingOther;

    [ObservableProperty]
    private string _otherNameInput = string.Empty;

    [ObservableProperty]
    private string _otherApiKeyInput = string.Empty;

    [ObservableProperty]
    private string _otherEndpointInput = string.Empty;

    public ProvidersWindowViewModel(SessionContext sessionContext, IGatewayHandler gatewayHandler,
        INotificationService notificationService)
    {
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;

        foreach (var kind in Enum.GetValues<AuthorizedLlmProvider>())
        {
            if (kind == AuthorizedLlmProvider.Other)
            {
                continue;
            }

            var metadata = ProviderMetadata[kind];
            KnownProviders.Add(new ProviderCardItem(kind, metadata.Name, metadata.DisplayName, ToIconUrl(metadata.Domain)));
        }

        ModelAssignments.Add(new Models.ModelAssignmentSlot(
            ModelAssignmentType.Primary,
            "Primary",
            "Primary model",
            Localizer.Instance?["ModelAssignments.Primary"] ?? string.Empty,
            true,
            _sessionContext, _gatewayHandler, _notificationService, GetConfiguredProviders));

        ModelAssignments.Add(new Models.ModelAssignmentSlot(
            ModelAssignmentType.Triage,
            "Triage",
            "Triage model",
            Localizer.Instance?["ModelAssignments.Triage"] ?? string.Empty,
            false,
            _sessionContext, _gatewayHandler, _notificationService, GetConfiguredProviders));

        ModelAssignments.Add(new Models.ModelAssignmentSlot(
            ModelAssignmentType.Discovery,
            "Discovery",
            "Discovery model",
            Localizer.Instance?["ModelAssignments.Discovery"] ?? string.Empty,
            false,
            _sessionContext, _gatewayHandler, _notificationService, GetConfiguredProviders));
    }

    private IEnumerable<Provider> GetConfiguredProviders()
    {
        return KnownProviders.Where(k => k.IsConfigured).Select(k => k.Configured!).Concat(OtherProviders);
    }

    public override async Task Initialize()
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        try
        {
            var configured = await _gatewayHandler.HandleGetAllProviders(gateway, profile.Id);
            var knownValues = KnownProviders.Select(k => k.Name).ToHashSet();

            foreach (var card in KnownProviders)
            {
                card.Configured = configured.FirstOrDefault(p => Normalize(p.Name) == card.Name);
                card.IsAdding = false;
            }

            OtherProviders.Clear();
            foreach (var provider in configured.Where(p => !knownValues.Contains(Normalize(p.Name))))
            {
                OtherProviders.Add(provider);
            }

            IsAddingOther = false;

            var assignments = await _gatewayHandler.HandleGetAllModelAssignments(gateway, profile.Id);

            foreach (var slot in ModelAssignments)
            {
                var assignment = assignments.FirstOrDefault(a => a.Role == slot.Role);
                var providerName = assignment is null ? null : ResolveProviderName(assignment.ProviderId, configured);
                slot.ApplyAssignment(assignment, providerName);
            }
        }
        catch (HttpRequestException)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.LoadFailed.Title"],
                Localizer.Instance!["Providers.LoadFailed.Message"]);
        }
    }

    private static string? ResolveProviderName(string providerId, List<Provider> configured)
    {
        return configured.FirstOrDefault(p => p.Id == providerId)?.Name;
    }

    private void ShowActionFailed()
    {
        _notificationService.ShowError(
            Localizer.Instance?["Providers.ActionFailed.Title"],
            Localizer.Instance!["Providers.ActionFailed.Message"]);
    }

    [RelayCommand]
    private void ShowAddForm(ProviderCardItem item)
    {
        item.ApiKeyInput = string.Empty;
        item.EndpointInput = string.Empty;
        item.IsAdding = true;
    }

    [RelayCommand]
    private void CancelAdd(ProviderCardItem item)
    {
        item.IsAdding = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task ConfirmAdd(ProviderCardItem item)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(item.ApiKeyInput))
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.ApiKeyRequired.Title"],
                Localizer.Instance!["Providers.ApiKeyRequired.Message"]);
            return;
        }

        var provider = new Provider
        {
            Name = item.Name,
            ApiKey = item.ApiKeyInput.Trim(),
            EndpointUrl = string.IsNullOrWhiteSpace(item.EndpointInput) ? null : item.EndpointInput.Trim()
        };

        Provider? created;
        try
        {
            created = await _gatewayHandler.HandleAddProvider(gateway, provider, profile.Id);
        }
        catch (HttpRequestException)
        {
            created = null;
        }

        if (created is null)
        {
            ShowActionFailed();
            return;
        }

        item.Configured = created;
        item.IsAdding = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Remove(ProviderCardItem item)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null || item.Configured is null)
        {
            return;
        }

        bool success;
        try
        {
            success = await _gatewayHandler.HandleDeleteProvider(gateway, item.Configured, profile.Id);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            ShowActionFailed();
            return;
        }

        item.Configured = null;
    }

    [RelayCommand]
    private void ShowAddOther()
    {
        OtherNameInput = string.Empty;
        OtherApiKeyInput = string.Empty;
        OtherEndpointInput = string.Empty;
        IsAddingOther = true;
    }

    [RelayCommand]
    private void CancelAddOther()
    {
        IsAddingOther = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task ConfirmAddOther()
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(OtherNameInput))
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.NameRequired.Title"],
                Localizer.Instance!["Providers.NameRequired.Message"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(OtherApiKeyInput))
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.ApiKeyRequired.Title"],
                Localizer.Instance!["Providers.ApiKeyRequired.Message"]);
            return;
        }

        var provider = new Provider
        {
            Name = nameof(AuthorizedLlmProvider.Other).ToLower(),
            ApiKey = OtherApiKeyInput.Trim(),
            EndpointUrl = string.IsNullOrWhiteSpace(OtherEndpointInput) ? null : OtherEndpointInput.Trim()
        };

        Provider? created;
        try
        {
            created = await _gatewayHandler.HandleAddProvider(gateway, provider, profile.Id, OtherNameInput.Trim());
        }
        catch (HttpRequestException)
        {
            created = null;
        }

        if (created is null)
        {
            ShowActionFailed();
            return;
        }

        created.Name = OtherNameInput.Trim();
        OtherProviders.Add(created);
        IsAddingOther = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task RemoveOther(Provider provider)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        bool success;
        try
        {
            success = await _gatewayHandler.HandleDeleteProvider(gateway, provider, profile.Id);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            ShowActionFailed();
            return;
        }

        OtherProviders.Remove(provider);
    }

    private static readonly Dictionary<AuthorizedLlmProvider, (string Name, string DisplayName, string Domain)> ProviderMetadata = new()
    {
        [AuthorizedLlmProvider.DeepSeek] = ("deep-seek", "DeepSeek", "deepseek.com"),
        [AuthorizedLlmProvider.Anthropic] = ("anthropic", "Anthropic", "anthropic.com"),
        [AuthorizedLlmProvider.OpenRouter] = ("open-router", "OpenRouter", "openrouter.ai"),
        [AuthorizedLlmProvider.OpenAI] = ("open-ai", "OpenAI", "openai.com"),
        [AuthorizedLlmProvider.Gemini] = ("gemini", "Gemini", "gemini.google.com"),
        [AuthorizedLlmProvider.Mistral] = ("mistral", "Mistral", "mistral.ai"),
        [AuthorizedLlmProvider.XAI] = ("xai", "xAI", "x.ai"),
        [AuthorizedLlmProvider.Other] = ("other", "Other", string.Empty)
    };

    private static string ToIconUrl(string domain)
    {
        return $"https://www.google.com/s2/favicons?sz=64&domain={domain}";
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
