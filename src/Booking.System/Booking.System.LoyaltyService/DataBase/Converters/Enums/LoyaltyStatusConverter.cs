using Booking.System.LoyaltyService.Core.Models.Enums;
using DbLoyaltyStatus = Booking.System.LoyaltyService.DataBase.Models.Enums.LoyaltyStatus;

namespace Booking.System.LoyaltyService.DataBase.Converters.Enums;

public class LoyaltyStatusConverter
{
    public static LoyaltyStatus Convert(DbLoyaltyStatus loyalty)
    {
        return loyalty switch
        {
            DbLoyaltyStatus.GOLD => LoyaltyStatus.GOLD,
            DbLoyaltyStatus.SILVER => LoyaltyStatus.SILVER,
            DbLoyaltyStatus.BRONZE => LoyaltyStatus.BRONZE,
            _ => LoyaltyStatus.BRONZE
        };
    }
    
    public static DbLoyaltyStatus Convert(LoyaltyStatus loyalty)
    {
        return loyalty switch
        {
            LoyaltyStatus.GOLD => DbLoyaltyStatus.GOLD,
            LoyaltyStatus.SILVER => DbLoyaltyStatus.SILVER,
            LoyaltyStatus.BRONZE => DbLoyaltyStatus.BRONZE,
            _ => DbLoyaltyStatus.BRONZE
        };
    }
}