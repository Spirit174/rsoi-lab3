using System.Reflection;
using Booking.System.PaymentService.DataBase.Context;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.PaymentService.Extensions;

public static class HostProviderExtensions
{
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<PaymentContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
    }
}