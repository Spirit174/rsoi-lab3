using Booking.System.ReservationService.Core.Models.Enums;

namespace Booking.System.ReservationService.DTO.Converters.Enums;

public class ReservationStatusConverter
{
    public static string Convert(PaymentStatus loyalty)
    {
        return loyalty switch
        {
            PaymentStatus.PAID => "PAID",
            PaymentStatus.CANCELED => "CANCELED",
            _ => "PAID"
        };
    }
}