using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Factory;

namespace Kori.ViewModels;

public partial class ConnectionErrorWindowViewModel : ViewModelBase
{
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;

    public ConnectionErrorWindowViewModel(INavigationHandler navigationHandler, ViewFactory viewFactory)
    {
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
    }

    [RelayCommand]
    private void Reload()
    {
        _navigationHandler.ClearBackStack();
        _navigationHandler.NavigateTo(_viewFactory.GetView<MainWindowViewModel>());
    }
}
