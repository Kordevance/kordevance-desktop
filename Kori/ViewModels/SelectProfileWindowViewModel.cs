using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;
using Kori.Validators;

namespace Kori.ViewModels;

public sealed partial class SelectProfileWindowViewModel : ResultViewModel<Profile>
{
    private readonly Gateway _gateway;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly INavigationHandler _navigationHandler;
    private readonly INotificationService _notificationService;
    
    public IReadOnlyList<ProfileTile> Tiles { get; }

    public bool HasProfiles => Tiles.Count > 1;

    [ObservableProperty]
    private bool _isCreatingProfile;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [LocalizedRequired("Profile.NameRequired")]
    private string _name = string.Empty;

    public SelectProfileWindowViewModel(Gateway gateway, List<Profile> profiles, IGatewayHandler gatewayHandler,
        INavigationHandler navigationHandler, INotificationService notificationService)
    {
        _gateway = gateway;
        _gatewayHandler = gatewayHandler;
        _navigationHandler = navigationHandler;
        _notificationService = notificationService;

        var all = new Profile?[] { null }.Concat(profiles).ToList();
        Tiles = all.Select((profile, index) => new ProfileTile(profile, index == all.Count - 1)).ToList();

        IsCreatingProfile = profiles.Count == 0;
    }
    
    [RelayCommand]
    private void TileClicked(Profile? profile)
    {
        if (profile is null)
        {
            ShowCreateProfile();
        }
        else
        {
            TryComplete(profile);
            _navigationHandler.NavigateBack();
        }
    }

    [RelayCommand]
    private void ShowCreateProfile()
    {
        Name = string.Empty;
        ClearErrors();
        IsCreatingProfile = true;
    }

    [RelayCommand]
    private void ShowProfileList()
    {
        ClearErrors();
        IsCreatingProfile = false;
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task CreateProfile()
    {
        ValidateProperty(Name, nameof(Name));
        if (HasErrors)
        {
            return;
        }

        var profile = await _gatewayHandler.HandleCreateProfile(_gateway, Name);
        if (profile is null)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Profile.CreateFailed.Title"],
                Localizer.Instance!["Profile.CreateFailed.Message"]
            );
            return;
        }

        TryComplete(profile);
        _navigationHandler.NavigateBack();
    }
}

public sealed record ProfileTile(Profile? Profile, bool IsLast);
