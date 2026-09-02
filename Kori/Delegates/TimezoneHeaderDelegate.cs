using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kori.Delegates;

public sealed class TimezoneHeaderDelegate : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("X-Profile-Timezone");
        request.Headers.Add("X-Profile-Timezone", TimeZoneInfo.Local.Id);

        return base.SendAsync(request, cancellationToken);
    }
}
