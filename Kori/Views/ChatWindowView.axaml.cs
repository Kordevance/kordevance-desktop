using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Kori.ViewModels;

namespace Kori.Views;

public partial class ChatWindowView : UserControl
{
    public ChatWindowView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is ChatWindowViewModel viewModel)
        {
            viewModel.Bubbles.CollectionChanged -= OnBubblesChanged;
            viewModel.Bubbles.CollectionChanged += OnBubblesChanged;
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnBubblesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ScrollToEnd();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatWindowViewModel.IsSending))
        {
            ScrollToEnd();
        }
    }

    private void ScrollToEnd()
    {
        Dispatcher.UIThread.Post(() => MessagesScrollViewer.ScrollToEnd(), DispatcherPriority.Loaded);
    }

    private void OnDraftMessageKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        if (DataContext is ChatWindowViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
        {
            viewModel.SendMessageCommand.Execute(null);
        }

        e.Handled = true;
    }
}
