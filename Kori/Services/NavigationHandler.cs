using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kori.Contracts;
using Kori.Models;
using Kori.ViewModels;

namespace Kori.Services;

public sealed class NavigationHandler : INavigationHandler
{
    private ViewModelBase? _currentViewModel;
    private readonly Stack<ViewModelBase> _stack = new();
    
    public event Action<ViewModelBase>? Navigated;
    public event Action<AppShellPages>? Navigating;

    public void SwitchToPage(AppShellPages page)
    {
        Navigating?.Invoke(page);
    }
    
    public void NavigateTo(ViewModelBase viewModel, bool keepTrack=true)
    {
        if (_currentViewModel != null && _currentViewModel != viewModel)
        {
            if (keepTrack)
            {
                _stack.Push(_currentViewModel);
            }
        }
        
        _currentViewModel = viewModel;
        Navigated?.Invoke(_currentViewModel);
    }

    public Task<T> NavigateTo<T>(ResultViewModel<T> viewModel)
    {
        NavigateTo(viewModel as ViewModelBase ?? throw new ArgumentException(nameof(viewModel)));

        return viewModel.Result;
    }

    public void NavigateBack()
    {
        if (_stack.Count == 0)
        {
            return;
        }

        _currentViewModel = _stack.Pop();
        Navigated?.Invoke(_currentViewModel);
    }

    public void ClearBackStack()
    {
        _stack.Clear();
    }
}