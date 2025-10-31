using Booking.System.LoyaltyService.Core.Models.Enums;

namespace Booking.System.LoyaltyService.DTO.Converters.Enums;

public class LoyaltyInfoDtoStatusConverter
{
    public static string Convert(LoyaltyStatus loyalty)
    {
        return loyalty switch
        {
            LoyaltyStatus.GOLD => "GOLD",
            LoyaltyStatus.SILVER => "SILVER",
            LoyaltyStatus.BRONZE => "BRONZE",
            _ => "BRONZE"
        };
    }
}
