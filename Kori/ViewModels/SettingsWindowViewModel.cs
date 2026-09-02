using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;

namespace Kori.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INotificationService _notificationService;
    private readonly IClipboardService _clipboardService;

    public ObservableCollection<Device> Devices { get; } = new();

    [ObservableProperty] private string _profileName = string.Empty;
    [ObservableProperty] private string _gatewayAddress = string.Empty;

    [ObservableProperty] private string? _pairingCode;
    [ObservableProperty] private bool _isGeneratingPairingCode;

    public SettingsWindowViewModel(SessionContext sessionContext, IGatewayHandler gatewayHandler,
        INotificationService notificationService, IClipboardService clipboardService)
    {
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;
        _clipboardService = clipboardService;
    }

    public override async Task Initialize()
    {
        PairingCode = null;

        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;

        ProfileName = profile?.Name ?? string.Empty;
        GatewayAddress = gateway?.Address ?? string.Empty;

        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        var gateway = _sessionContext.Gateway;
        if (gateway is null)
        {
            return;
        }

        try
        {
            var devices = await _gatewayHandler.HandleRegisteredDevices(gateway);

            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }
        }
        catch (HttpRequestException)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Settings.Devices.LoadFailed.Title"],
                Localizer.Instance!["Settings.Devices.LoadFailed.Message"]);
        }
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task CreatePairingCode()
    {
        var gateway = _sessionContext.Gateway;
        if (gateway is null || IsGeneratingPairingCode)
        {
            return;
        }

        IsGeneratingPairingCode = true;

        try
        {
            PairingCode = await _gatewayHandler.HandleCreatePairingInvite(gateway);
        }
        catch (HttpRequestException)
        {
            PairingCode = null;
            _notificationService.ShowError(
                Localizer.Instance?["Settings.Pairing.CreateFailed.Title"],
                Localizer.Instance!["Settings.Pairing.CreateFailed.Message"]);
        }
        finally
        {
            IsGeneratingPairingCode = false;
        }
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task CopyPairingCode()
    {
        if (string.IsNullOrEmpty(PairingCode))
        {
            return;
        }

        await _clipboardService.SetTextAsync(PairingCode);
        _notificationService.ShowSuccess(
            null,
            Localizer.Instance!["Settings.Pairing.Copied"]);
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task PromoteDevice(Device device)
    {
        var gateway = _sessionContext.Gateway;
        if (gateway is null || device.IsOwner)
        {
            return;
        }

        bool success;
        try
        {
            success = await _gatewayHandler.HandlePromoteDeviceToOwner(gateway, device);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Settings.Devices.PromoteFailed.Title"],
                Localizer.Instance!["Settings.Devices.PromoteFailed.Message"]);
            return;
        }

        await LoadDevices();
    }
}
