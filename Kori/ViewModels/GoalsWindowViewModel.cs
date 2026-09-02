using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;

namespace Kori.ViewModels;

public partial class GoalsWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INotificationService _notificationService;

    public ObservableCollection<Goal> Goals { get; } = new();

    public GoalsWindowViewModel(SessionContext sessionContext, IGatewayHandler gatewayHandler,
        INotificationService notificationService)
    {
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _notificationService = notificationService;
    }

    public override async Task Initialize()
    {
        await LoadGoals();
    }

    private async Task LoadGoals()
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        try
        {
            var goals = await _gatewayHandler.HandleGetAllGoals(gateway, profile.Id);

            Goals.Clear();
            foreach (var goal in goals)
            {
                Goals.Add(goal);
            }
        }
        catch (HttpRequestException)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Goals.LoadFailed.Title"],
                Localizer.Instance!["Goals.LoadFailed.Message"]);
        }
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task DeleteGoal(Goal goal)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null)
        {
            return;
        }

        var success = await _gatewayHandler.HandleDeleteGoal(gateway, goal, profile.Id);
        if (!success)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Goals.DeleteFailed.Title"],
                Localizer.Instance!["Goals.DeleteFailed.Message"]);
            return;
        }

        Goals.Remove(goal);
    }
}
