using Booking.System.ReservationService.DataBase.Context;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.ReservationService.Extensions;

public static class HostProviderExtensions
{
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<ReservationContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
    }
}