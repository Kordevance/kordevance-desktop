using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kori.Contracts;
using Kori.Models;

namespace Kori.ViewModels;


public partial class ConnectorsWindowViewModel : ViewModelBase
{
    private readonly SessionContext _sessionContext;
    private readonly IGatewayHandler _gatewayHandler;
    private readonly IUrlLauncher _urlLauncher;
    private readonly INotificationService _notificationService;

    [ObservableProperty] private ObservableCollection<ConnectorGroup> _connectorGroup = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchText))]
    private string _searchText = string.Empty;

    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    private List<ConnectorGroup> _allGroups = [];

    public ConnectorsWindowViewModel(SessionContext sessionContext, IGatewayHandler gatewayHandler,
        IUrlLauncher urlLauncher, INotificationService notificationService)
    {
        _sessionContext = sessionContext;
        _gatewayHandler = gatewayHandler;
        _urlLauncher = urlLauncher;
        _notificationService = notificationService;
    }


    public override async Task Initialize()
    {
        if (_sessionContext.Gateway is null || _sessionContext.Profile is null) return;

        var connectors =
            await _gatewayHandler.HandleGetAllConnectors(_sessionContext.Gateway, _sessionContext.Profile.Id);

        _allGroups = connectors
            .GroupBy(c => c.Provider)
            .Select(g => new ConnectorGroup
            {
                Provider = g.Key,
                Icon = g.First().Icon,
                Entries = new ObservableCollection<Connector>(g.OrderBy(c => c.Category))
            })
            .OrderBy(g => g.Provider)
            .ToList();

        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private void ApplyFilter()
    {
        var term = Normalize(SearchText);

        if (term.Length == 0)
        {
            ConnectorGroup = new ObservableCollection<ConnectorGroup>(_allGroups);
            return;
        }

        var filtered = new List<ConnectorGroup>();

        foreach (var group in _allGroups)
        {
            if (Normalize(group.Provider).Contains(term))
            {
                filtered.Add(group);
                continue;
            }

            var matchingEntries = group.Entries.Where(e => Normalize(e.Category).Contains(term)).ToList();
            if (matchingEntries.Count > 0)
            {
                filtered.Add(new ConnectorGroup
                {
                    Provider = group.Provider,
                    Icon = group.Icon,
                    Entries = new ObservableCollection<Connector>(matchingEntries)
                });
            }
        }

        ConnectorGroup = new ObservableCollection<ConnectorGroup>(filtered);
    }

    private static string Normalize(string value)
    {
        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Connect(Connector connector)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null) return;

        string url;
        try
        {
            url = await _gatewayHandler.HandleAddConnector(gateway, connector, profile.Id);
        }
        catch (HttpRequestException)
        {
            url = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(url) || !_urlLauncher.Open(url))
        {
            _notificationService.ShowError(
                Localizer.Instance?["Connectors.ActionFailed.Title"],
                Localizer.Instance!["Connectors.ActionFailed.Message"]);
        }
    }

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task Disconnect(Connector connector)
    {
        var gateway = _sessionContext.Gateway;
        var profile = _sessionContext.Profile;
        if (gateway is null || profile is null) return;

        bool success;
        try
        {
            success = await _gatewayHandler.HandleDeleteConnector(gateway, connector, profile.Id);
        }
        catch (HttpRequestException)
        {
            success = false;
        }

        if (!success)
        {
            _notificationService.ShowError(
                Localizer.Instance?["Connectors.ActionFailed.Title"],
                Localizer.Instance!["Connectors.ActionFailed.Message"]);
            return;
        }

        connector.Active = false;
    }
}
