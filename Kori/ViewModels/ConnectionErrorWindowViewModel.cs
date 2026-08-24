using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Factory;

namespace Kori.ViewModels;

public partial class ConnectionErrorWindowViewModel : ViewModelBase
{
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;
    private readonly SessionContext _sessionContext;

    public ConnectionErrorWindowViewModel(INavigationHandler navigationHandler, ViewFactory viewFactory, SessionContext sessionContext)
    {
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
        _sessionContext = sessionContext;
    }

    [RelayCommand]
    private void Reload()
    {
        _navigationHandler.ClearBackStack();
        _navigationHandler.NavigateTo(_viewFactory.GetView<MainWindowViewModel>());
    }

    [RelayCommand]
    private void Logout()
    {
        _sessionContext.Logout();
        _navigationHandler.ClearBackStack();
        _navigationHandler.NavigateTo(_viewFactory.GetView<GatewayRegistrationWindowViewModel>());
    }
}
