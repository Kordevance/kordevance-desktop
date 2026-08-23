using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Factory;
using Kori.Models;

namespace Kori.ViewModels;

public partial class AppShellWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;
    private readonly IGatewayHandler _gatewayHandler;

    [ObservableProperty] private AppShellPages _currentPage;
    [ObservableProperty] private ViewModelBase? _currentPageContent;

    public AppShellWindowViewModel(SessionContext sessionContext, INavigationHandler navigationHandler,
        ViewFactory viewFactory, IGatewayHandler gatewayHandler)
    {
        _sessionContext = sessionContext;
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
        _gatewayHandler = gatewayHandler;
        

        _navigationHandler.Navigating += page => _ = NavigateToPage(page);

        _ = Initialize();
    }

    public sealed override async Task Initialize()
    {
        await EnsureProfileSelected();
        await NavigateToPage(AppShellPages.Chat);
    }

    private async Task NavigateToPage(AppShellPages page)
    {
        var viewModel = ResolvePage(page);
        await viewModel.Initialize();

        CurrentPage = page;
        CurrentPageContent = viewModel;
    }

    private ViewModelBase ResolvePage(AppShellPages page)
    {
        return page switch
        {
            AppShellPages.Connectors => _viewFactory.GetView<ConnectorsWindowViewModel>(),
            AppShellPages.Chat => _viewFactory.GetView<ChatWindowViewModel>(),
            AppShellPages.Goals => _viewFactory.GetView<GoalsWindowViewModel>(),
            AppShellPages.Settings => _viewFactory.GetView<SettingsWindowViewModel>(),
            AppShellPages.Providers => _viewFactory.GetView<ProvidersWindowViewModel>(),
            _ => _viewFactory.GetView<ChatWindowViewModel>()
        };
    }

    [RelayCommand]
    private void SwitchPage(AppShellPages page)
    {
        _navigationHandler.SwitchToPage(page);
    }

    [RelayCommand]
    private void Logout()
    {
        _sessionContext.Logout();
        _ = EnsureProfileSelected();
    }

    private async Task EnsureProfileSelected()
    {
        if (_sessionContext.Profile is not null)
        {
            return;
        }

        var gateway = _sessionContext.Gateway;
        if (gateway is null) return;

        var profiles = await _gatewayHandler.HandleGetAllProfiles(gateway) ?? [];

        var profile = await _navigationHandler.NavigateTo(
            _viewFactory.GetView<SelectProfileWindowViewModel>(gateway, profiles));

        _sessionContext.Profile = profile;
    }
}
