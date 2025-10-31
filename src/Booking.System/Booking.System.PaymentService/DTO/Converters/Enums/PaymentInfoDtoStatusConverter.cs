using Booking.System.PaymentService.Core.Models.Enums;

namespace Booking.System.PaymentService.DTO.Converters.Enums;

public class PaymentInfoDtoStatusConverter
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