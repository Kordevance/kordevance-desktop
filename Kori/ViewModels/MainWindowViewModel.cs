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
    
    public string Title { get; private set; }

    [ObservableProperty] private ViewModelBase? _currentView;
    
    public MainWindowViewModel(SessionContext sessionContext, ViewFactory viewFactory, INavigationHandler navigationHandler, IConfiguration configuration)
    {
        Title = configuration["Name"] ?? throw new NullReferenceException("Application name is not configured");
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
        _sessionContext = sessionContext;
        
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

    public override Task Initialize()
    {
        _sessionContext.Initialize();
        if (_sessionContext.IsConnectedToGateway)
        {
            _navigationHandler.NavigateTo(_viewFactory.GetView<AppShellWindowViewModel>());
        }
        else
        {
            _navigationHandler.NavigateTo(_viewFactory.GetView<GatewayRegistrationWindowViewModel>());
        }
        return Task.CompletedTask;
    }
}