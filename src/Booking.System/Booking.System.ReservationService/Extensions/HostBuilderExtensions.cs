using Serilog;

namespace Booking.System.ReservationService.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder SetupSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((hostingContext, configuration) =>
        {
            configuration.ReadFrom.Configuration(hostingContext.Configuration);
        });
    }
}