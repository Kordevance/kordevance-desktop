using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Kori.Contracts;
using Kori.Factory;
using Microsoft.Extensions.Configuration;

namespace Kori.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;
    private readonly SessionContext _sessionContext;
    private readonly AppVersioning _appVersioning;
    
    public string Title { get; private set; }

    [ObservableProperty] private ViewModelBase? _currentView;
    
    public MainWindowViewModel(AppVersioning apv, SessionContext sessionContext, ViewFactory viewFactory, INavigationHandler navigationHandler, IConfiguration configuration)
    {
        Title = configuration["Name"] ?? throw new NullReferenceException("Application name is not configured");
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
        _sessionContext = sessionContext;
        _appVersioning = apv;
        
        navigationHandler.ClearBackStack();
        navigationHandler.Navigated += viewModel =>
        {
            if (viewModel is MainWindowViewModel && _sessionContext.IsConnectedToGateway)
            {
                
                _navigationHandler.NavigateTo(_viewFactory.GetView<AppShellWindowViewModel>());
            }
            else
            {
                CurrentView = viewModel;
            }
        };

    }

    public override async Task Initialize()
    {
        if (await _appVersioning.CheckForUpdatesAsync())
        {
            _appVersioning.ApplyUpdateAndRestart();
        }
        
        await _sessionContext.Initialize();
        if (_sessionContext.IsConnectedToGateway)
        {
            _navigationHandler.NavigateTo(_viewFactory.GetView<AppShellWindowViewModel>());
        }
        else
        {
            _navigationHandler.NavigateTo(_viewFactory.GetView<GatewayRegistrationWindowViewModel>());
        }
    }
}