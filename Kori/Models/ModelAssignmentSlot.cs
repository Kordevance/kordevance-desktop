using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;

namespace Kori.Models;

public sealed partial class ModelAssignmentSlot : ObservableObject
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INotificationService _notificationService;
    private readonly Func<IEnumerable<Provider>> _getConfiguredProviders;

    public ModelAssignmentType Role { get; }
    public string Title { get; }
    public string InfoTitle { get; }
    public string InfoMessage { get; }
    public bool IsRequired { get; }

    public ObservableCollection<string> AvailableModels { get; } = new();

    public IEnumerable<Provider> ConfiguredProviders => _getConfiguredProviders();

    [ObservableProperty]
    private string? _providerId;

    [ObservableProperty]
    private string? _providerName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAssigned))]
    private string? _modelId;

    [ObservableProperty]
    private bool _isPicking;

    [ObservableProperty]
    private Provider? _pickerProvider;

    [ObservableProperty]
    private string? _pickerModel;

    [ObservableProperty]
    private bool _noModelsFound;

    [ObservableProperty]
    private string _manualModelInput = string.Empty;

    public bool IsAssigned => !string.IsNullOrEmpty(ModelId);

    public ModelAssignmentSlot(ModelAssignmentType role, string title, string infoTitle, string infoMessage,
        bool isRequired, SessionContext sessionContext, IGatewayHandler gatewayHandler,
        INotificationService notificationService, Func<IEnumerable<Provider>> getConfiguredProviders)
    {
        Role = role;
        Title = title;
        InfoTitle = infoTitle;
        InfoMessage = infoMessage;
        IsRequired = isRequired;
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;
        _getConfiguredProviders = getConfiguredProviders;
    }

    public void ApplyAssignment(ModelAssignment? assignment, string? providerName)
    {
        ProviderId = assignment?.ProviderId;
        ModelId = assignment?.ModelId;
        ProviderName = providerName;
        IsPicking = false;
    }

    [RelayCommand]
    private void ShowPicker()
    {
        OnPropertyChanged(nameof(ConfiguredProviders));
        PickerProvider = null;
        PickerModel = null;
        NoModelsFound = false;
        ManualModelInput = string.Empty;
        AvailableModels.Clear();
        IsPicking = true;
    }

    [RelayCommand]
    private void CancelPicker()
    {
        IsPicking = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task SelectProvider(Provider provider)
    {
        PickerProvider = provider;
        PickerModel = null;
        NoModelsFound = false;
        ManualModelInput = string.Empty;
        AvailableModels.Clear();

        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        List<string> models;
        try
        {
            models = await _gatewayHandler.HandleGetProviderModels(gateway, provider, profile.Id);
        }
        catch (HttpRequestException)
        {
            models = [];
        }

        foreach (var model in models)
        {
            AvailableModels.Add(model);
        }

        NoModelsFound = models.Count == 0;
    }

    [RelayCommand]
    private void SelectModel(string model)
    {
        PickerModel = model;
    }

    [RelayCommand]
    private void UseManualModel()
    {
        if (string.IsNullOrWhiteSpace(ManualModelInput))
        {
            return;
        }

        PickerModel = ManualModelInput.Trim();
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Confirm()
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null || PickerProvider is null || string.IsNullOrWhiteSpace(PickerModel))
        {
            return;
        }

        bool success;
        try
        {
            success = await _gatewayHandler.HandleAssignModel(gateway, PickerProvider.Id, PickerModel, Role, profile.Id);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.ActionFailed.Title"],
                Localizer.Instance!["Providers.ActionFailed.Message"]);
            return;
        }

        ProviderId = PickerProvider.Id;
        ProviderName = PickerProvider.Name;
        ModelId = PickerModel;
        IsPicking = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Clear()
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
            success = await _gatewayHandler.HandleDeleteModelAssignment(gateway, Role, profile.Id);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Providers.ActionFailed.Title"],
                Localizer.Instance!["Providers.ActionFailed.Message"]);
            return;
        }

        ProviderId = null;
        ProviderName = null;
        ModelId = null;
    }

    [RelayCommand]
    private void ShowInfo()
    {
        _notificationService.ShowInfo(InfoTitle, InfoMessage);
    }
}
