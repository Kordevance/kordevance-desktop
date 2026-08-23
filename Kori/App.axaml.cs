using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Kori.Contracts;
using Kori.Services;
using Kori.ViewModels;
using Kori.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kori;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Localizer.Initialize();

        ImageLoader.AsyncImageLoader = new SvgAwareImageLoader();

        var services = new ServiceCollection();
        var configuration = new ConfigurationManager();
        
        configuration.AddJsonFile("appsettings.json", false, true);

        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationServices(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        var vm = serviceProvider.GetRequiredService<MainWindowViewModel>();
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        var clipboardService = serviceProvider.GetRequiredService<IClipboardService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindowView { DataContext = vm };
            notificationService.Initialize(mainWindow);
            clipboardService.Initialize(mainWindow);
            _ = vm.Initialize();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}