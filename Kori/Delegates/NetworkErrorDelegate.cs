using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kori.Contracts;
using Kori.Factory;
using Kori.ViewModels;

namespace Kori.Delegates;

public sealed class NetworkErrorDelegate : DelegatingHandler
{
    private readonly INavigationHandler _navigationHandler;
    private readonly ViewFactory _viewFactory;

    public NetworkErrorDelegate(INavigationHandler navigationHandler, ViewFactory viewFactory)
    {
        _navigationHandler = navigationHandler;
        _viewFactory = viewFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex) when (IsNetworkUnreachable(ex))
        {
            _navigationHandler.NavigateTo(_viewFactory.GetView<ConnectionErrorWindowViewModel>(), keepTrack: false);
            throw;
        }
    }

    private static bool IsNetworkUnreachable(HttpRequestException ex)
    {
        if (ex.HttpRequestError == HttpRequestError.ConnectionError)
        {
            return true;
        }

        return ex.InnerException is SocketException
        {
            SocketErrorCode: SocketError.NetworkUnreachable
                or SocketError.HostUnreachable
                or SocketError.ConnectionRefused
        };
    }
}
