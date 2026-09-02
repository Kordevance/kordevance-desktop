using Avalonia.Controls;

namespace Kori.Contracts;

/// <summary>
/// Defines the contract for displaying notifications to the user.
/// Provides methods for showing various types of notifications including info, success, warning, and error messages.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Initializes the notification service with the specified top-level window.
    /// </summary>
    /// <param name="topLevel">The top-level window where notifications will be displayed.</param>
    /// <remarks>
    /// This method must be called before any notification methods are used.
    /// It sets up the context needed for displaying notifications in the application.
    /// </remarks>
    void Initialize(TopLevel topLevel);
    
    /// <summary>
    /// Displays an informational notification to the user.
    /// </summary>
    /// <param name="title">The title of the notification. Can be null.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowInfo(string? title, string message);

    /// <summary>
    /// Displays a success notification to the user.
    /// </summary>
    /// <param name="title">The title of the notification. Can be null.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowSuccess(string? title, string message);

    /// <summary>
    /// Displays a warning notification to the user.
    /// </summary>
    /// <param name="title">The title of the notification. Can be null.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowWarning(string? title, string message);

    /// <summary>
    /// Displays an error notification to the user.
    /// </summary>
    /// <param name="title">The title of the notification. Can be null.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowError(string? title, string message); 
}