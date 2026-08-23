using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kori.Delegates;

public sealed class HttpLoggingDelegate : DelegatingHandler
{
    private readonly ILogger<HttpLoggingDelegate> _logger;
    
    public HttpLoggingDelegate(ILogger<HttpLoggingDelegate> logger)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        _logger.LogInformation("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            timer.Stop();
            _logger.LogInformation("Received HTTP response: {StatusCode} for {Method} {Uri} in {Elapsed}ms",
                response.StatusCode, request.Method, request.RequestUri, timer.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, "HTTP request failed after {Elapsed}ms: {Method} {Uri}",
                timer.ElapsedMilliseconds, request.Method, request.RequestUri);
            throw;
        }
    }
}