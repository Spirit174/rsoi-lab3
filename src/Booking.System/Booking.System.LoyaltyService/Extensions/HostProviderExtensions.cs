using Booking.System.LoyaltyService.DataBase.Context;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.LoyaltyService.Extensions;

public static class HostProviderExtensions
{
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<LoyaltyContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
    }
}