using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Factory;
using Kori.Validators;

namespace Kori.ViewModels;

public partial class GatewayRegistrationWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;
    private readonly IGatewayHandler  _gatewayHandler;
    private readonly INotificationService _notificationService;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [LocalizedRequired("Gateway.AddressRequired")]
    private string _gatewayAddress = string.Empty;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [LocalizedRequired("Gateway.CodeRequired")]
    private string _verificationCode = string.Empty;

    [ObservableProperty] private bool _isGatewayAddressSectionReadonly;
    [ObservableProperty] private bool _isVerificationCodeSectionVisible;

    public GatewayRegistrationWindowViewModel(SessionContext sessionContext, INavigationHandler navigationHandler, 
        ViewFactory viewFactory, IGatewayHandler gatewayHandler, INotificationService notificationService)
    {
        _sessionContext = sessionContext;
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    public async Task TestGatewayConnection()
    {
        if (IsVerificationCodeSectionVisible)
        {
            ValidateProperty(VerificationCode, nameof(VerificationCode));
            if (HasErrors)
            {
                return;
            }

            var gateway = await _gatewayHandler.HandleRegisterDeviceToGateway(GatewayAddress, VerificationCode);
            if (gateway is null)
            {
                _notificationService.ShowError(
                    Localizer.Instance?["Gateway.Unreachable.Title"],
                    Localizer.Instance!["Gateway.Unreachable.Message"]
                );
                return;
            }

            _sessionContext.SaveGateway(gateway);
            _navigationHandler.NavigateTo(_viewFactory.GetView<MainWindowViewModel>());
        }
        
        ValidateProperty(GatewayAddress, nameof(GatewayAddress));
        if (HasErrors)
        {
            return;
        }

        var reachable = await _gatewayHandler.HandleTestGatewayConnection(GatewayAddress);
        if (!reachable)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Gateway.Unreachable.Title"],
                Localizer.Instance!["Gateway.Unreachable.Message"]
                );
            return;
        }

        IsGatewayAddressSectionReadonly = true;
        IsVerificationCodeSectionVisible = true;
    }
}