using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;

namespace Kori.ViewModels;

public partial class ChatWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INotificationService _notificationService;
    private readonly IClipboardService _clipboardService;

    public ObservableCollection<Conversation> Conversations { get; } = new();

    public ObservableCollection<ChatTurn> Bubbles { get; } = new();

    [ObservableProperty]
    private Conversation? _selectedConversation;

    [ObservableProperty]
    private string _draftMessage = string.Empty;

    [ObservableProperty]
    private bool _isSending;

    public ChatWindowViewModel(SessionContext sessionContext, IGatewayHandler gatewayHandler,
        INotificationService notificationService, IClipboardService clipboardService)
    {
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;
        _clipboardService = clipboardService;
    }

    public override async Task Initialize()
    {
        await LoadConversations();
    }

    partial void OnSelectedConversationChanged(Conversation? value)
    {
        Bubbles.Clear();
        if (value is null)
        {
            return;
        }

        foreach (var turn in value.Messages)
        {
            Bubbles.Add(turn);
        }
    }

    private async Task LoadConversations()
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        try
        {
            var conversations = await _gatewayHandler.HandleGetAllChats(gateway, profile.Id);

            Conversations.Clear();
            foreach (var conversation in conversations)
            {
                Conversations.Add(conversation);
            }
        }
        catch (HttpRequestException)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Chat.LoadFailed.Title"],
                Localizer.Instance!["Chat.LoadFailed.Message"]);
        }
    }

    [RelayCommand]
    private void SelectConversation(Conversation? conversation)
    {
        SelectedConversation = conversation;
    }

    [RelayCommand]
    private async Task CopyMessage(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        await _clipboardService.SetTextAsync(content);
    }

    [RelayCommand]
    private void StartNewChat()
    {
        SelectedConversation = null;
        DraftMessage = string.Empty;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(DraftMessage) || IsSending)
        {
            return;
        }

        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        var text = DraftMessage;
        DraftMessage = string.Empty;
        IsSending = true;

        var pendingTurn = new ChatTurn { Role = ChatRole.User, Content = text };
        Bubbles.Add(pendingTurn);

        try
        {
            var chat = new Chat
            {
                ConversationId = SelectedConversation?.Id.ToString(),
                Message = text
            };

            var result = await _gatewayHandler.HandleChat(gateway, profile.Id, chat);
            if (result is null)
            {
                Bubbles.Remove(pendingTurn);
                DraftMessage = text;
                _notificationService.ShowError(
                    Localizer.Instance?["Chat.SendFailed.Title"],
                    Localizer.Instance!["Chat.SendFailed.Message"]);
                return;
            }

            if (SelectedConversation is null)
            {
                var id = Guid.TryParse(result.ConversationId, out var parsed) ? parsed : Guid.NewGuid();
                var conversation = new Conversation { Id = id, Messages = new List<ChatTurn>() };
                Append(conversation, text, result);
                Conversations.Insert(0, conversation);
                SelectedConversation = conversation;
            }
            else
            {
                Bubbles.Remove(pendingTurn);
                Append(SelectedConversation, text, result);
                Bubbles.Clear();
                foreach (var turn in SelectedConversation.Messages)
                {
                    Bubbles.Add(turn);
                }
            }
        }
        catch
        {
            Bubbles.Remove(pendingTurn);
            DraftMessage = text;
            _notificationService.ShowError(
                Localizer.Instance?["Chat.SendFailed.Title"],
                Localizer.Instance!["Chat.SendFailed.Message"]);
            throw;
        }
        finally
        {
            IsSending = false;
        }
    }

    private static void Append(Conversation conversation, string sentMessage, Chat result)
    {
        if (!string.IsNullOrWhiteSpace(sentMessage))
        {
            conversation.Messages.Add(new ChatTurn { Role = ChatRole.User, Content = sentMessage });
        }

        if (!string.IsNullOrWhiteSpace(result.Reply))
        {
            conversation.Messages.Add(new ChatTurn { Role = ChatRole.Assistant, Content = result.Reply! });
        }
    }
}
