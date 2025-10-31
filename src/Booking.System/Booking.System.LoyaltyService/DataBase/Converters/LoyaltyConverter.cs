using Booking.System.LoyaltyService.Core.Models;
using Booking.System.LoyaltyService.DataBase.Converters.Enums;
using Booking.System.LoyaltyService.DataBase.Models;

namespace Booking.System.LoyaltyService.DataBase.Converters;

public class LoyaltyConverter
{
    public static Loyalty Convert(DbLoyalty loyalty)
    {
        return new Loyalty(loyalty.Id,
            loyalty.Username,
            loyalty.ReservationCount,
            LoyaltyStatusConverter.Convert(loyalty.Status),
            loyalty.Discount);
    }
    
    public static DbLoyalty Convert(Loyalty loyalty)
    {
        return new DbLoyalty(loyalty.Id,
            loyalty.Username,
            loyalty.ReservationCount,
            LoyaltyStatusConverter.Convert(loyalty.Status),
            loyalty.Discount);
    }
}