using Booking.System.LoyaltyService.Core.Models;
using Booking.System.LoyaltyService.DTO.Converters.Enums;
using Booking.System.LoyaltyService.DTO.Models;

namespace Booking.System.LoyaltyService.DTO.Converters;

public class LoyaltyInfoDtoConverter
{
    public static LoyaltyInfoDto Convert(Loyalty loyalty)
    {
        return new LoyaltyInfoDto(LoyaltyInfoDtoStatusConverter.Convert(loyalty.Status),
            loyalty.Discount,
            loyalty.ReservationCount);
    }
}