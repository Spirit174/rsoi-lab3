using Serilog;
using Serilog.Extensions.Hosting;

namespace Booking.System.Gateway.Extensions;

public class SerilogLoggerFactory
{
    public static ReloadableLogger CreateProductionOrDefaultConfiguration(string configFilename,
        string consulSectionName = "ConsulConfiguration")
    {
        try
        {
            IConfiguration configuration;
            if (IsDevelopmentEnvironment())
            {
                configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json")
                    .Build();
            }
            else
            {
                configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFilename)
                    .Build();
            }

            return ConfigurationLoggerConfigurationExtensions
                .Configuration(configuration: configuration, settingConfiguration: new LoggerConfiguration().ReadFrom)
                .CreateBootstrapLogger();
        }
        catch (Exception)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
            throw;
        }
    }

    private static bool IsDevelopmentEnvironment()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != Environments.Development)
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
        }

        return true;
    }
}