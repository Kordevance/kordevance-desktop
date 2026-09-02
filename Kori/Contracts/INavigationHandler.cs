using System;
using System.Threading.Tasks;
using Kori.Models;
using Kori.ViewModels;

namespace Kori.Contracts;

/// <summary>
/// Defines the contract for navigation functionality in the application.
/// Provides methods to navigate between different views and manage navigation history.
/// </summary>
public interface INavigationHandler
{
    /// <summary>
    /// Occurs when navigation to a new view has been completed.
    /// </summary>
    /// <remarks>
    /// This event is raised with the new <see cref="ViewModelBase"/> instance after successful navigation.
    /// </remarks>
    event Action<ViewModelBase> Navigated;
    
    /// <summary>
    /// Occurs when navigation to a new view within the AppShell Container is requested.
    /// </summary>
    /// <remarks>
    /// This event is raised with the new <see cref="ViewModelBase"/> instance after successful navigation.
    /// </remarks>
    event Action<AppShellPages> Navigating;
    
    /// <summary>
    /// Switch to the specified page within the AppShell Container.
    /// </summary>
    /// <param name="page">The PAGE to navigate to.</param>
    /// <remarks>
    /// This method initiates navigation to the provided page and triggers the <see cref="Navigating"/> event upon completion.
    /// </remarks>
    void SwitchToPage(AppShellPages page);

    /// <summary>
    /// Navigates to the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to navigate to.</param>
    /// <param name="keepTrack">Flag indicating whether the current view (not the one to be navigated to) should be tracked and added to navigation history or not.</param>
    /// <remarks>
    /// This method initiates navigation to the provided view model and triggers the <see cref="Navigated"/> event upon completion.
    /// </remarks>
    void NavigateTo(ViewModelBase viewModel, bool keepTrack=true);

    /// <summary>
    /// Navigates to the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to navigate to.</param>
    /// <remarks>
    /// This method initiates navigation to the provided view model and triggers the <see cref="Navigated"/> event upon completion.
    /// </remarks>
    Task<T> NavigateTo<T>(ResultViewModel<T> viewModel);

    /// <summary>
    /// Navigates back to the previous view in the navigation history.
    /// </summary>
    /// <remarks>
    /// If there is no previous view in the history, this method has no effect.
    /// </remarks>
    void NavigateBack();
    
    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    /// <remarks>
    /// If there is no previous view in the history, this method has no effect.
    /// </remarks>
    void ClearBackStack();
}