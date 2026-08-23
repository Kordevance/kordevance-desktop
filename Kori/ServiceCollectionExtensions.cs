using System;
using Kori.Contracts;
using Kori.Delegates;
using Kori.Factory;
using Kori.Services;
using Kori.Store;
using Kori.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kori;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddSerilog(dispose: false));
        
        services.AddSingleton<ISecureStorageManager>(_ =>
        {
            var name = configuration["CredentialManagerKey"] ?? throw new NullReferenceException("Credential manager key is not configured");
            var key = $"com.{name.ToLower()}";
            if (OperatingSystem.IsWindows())
            {
                return new WindowsSecureStorageManager(key);
            }
            return new MacSecureStorageManager(key, AppConstants.IsReleaseEnvironment);
        });
        
        services.AddTransient<HttpLoggingDelegate>();
        services.AddTransient<NetworkErrorDelegate>();

        services.ConfigureHttpClientDefaults(builder =>
        {
            builder.RemoveAllLoggers();
            builder.AddHttpMessageHandler<HttpLoggingDelegate>();
            builder.AddHttpMessageHandler<NetworkErrorDelegate>();
        });

        services.AddSingleton<AppVersioning>();
        services.AddSingleton<SessionContext>();
        services.AddTransient<IGatewayHandler, GatewayHandler>();
        
        services.AddSingleton<ViewFactory>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<INavigationHandler, NavigationHandler>();
        services.AddSingleton<IUrlLauncher, UrlLauncher>();
        
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddTransient<GatewayRegistrationWindowViewModel>();
        services.AddTransient<AppShellWindowViewModel>();
        services.AddTransient<SelectProfileWindowViewModel>();
        services.AddTransient<ConnectionErrorWindowViewModel>();
        services.AddTransient<ConnectorsWindowViewModel>();
        services.AddTransient<ChatWindowViewModel>();
        services.AddTransient<GoalsWindowViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<ProvidersWindowViewModel>();
    }
}