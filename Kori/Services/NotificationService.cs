using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Kori.Contracts;

namespace Kori.Services;

public sealed class NotificationService : INotificationService
{
    private WindowNotificationManager? _manager;

    public void Initialize(TopLevel topLevel)
    {
        _manager = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3
        };
    }
    
    public void ShowSuccess(string? title, string message) =>
        Show(title ?? "Success", message, NotificationType.Success);

    public void ShowError(string? title, string message) =>
        Show(title ?? "Error", message, NotificationType.Error);

    public void ShowInfo(string? title, string message) =>
        Show(title ?? "Info", message, NotificationType.Information);

    public void ShowWarning(string? title, string message) =>
        Show(title ?? "Warning", message, NotificationType.Warning);

    private void Show(string title, string message, NotificationType type) =>
        _manager?.Show(new Notification(title, message, type, TimeSpan.FromSeconds(7)));
}