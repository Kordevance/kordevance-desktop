using Avalonia;
using System;
using System.Threading.Tasks;
using Serilog;
using Velopack;

namespace Kori;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        
        Logging.Configure();
        HookGlobalExceptionHandlers();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void HookGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception,
                "Unhandled exception (terminating: {IsTerminating})", e.IsTerminating);
        };

        // Fires for exceptions from fire-and-forget async work e.g. an
        // [ObservableProperty]/[RelayCommand] async method whose returned Task nobody awaited.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}
