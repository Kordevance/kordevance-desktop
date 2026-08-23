using Serilog;

namespace Kori;

public static class Logging
{
    public static void Configure()
    {
        var config = new LoggerConfiguration();

        if (!AppConstants.IsReleaseEnvironment)
        {
            config.MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            config.MinimumLevel.Warning()
                .WriteTo.File("logs/app.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 3,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        Log.Logger = config.CreateLogger();
    }
}
