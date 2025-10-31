using Booking.System.ReservationService.Core.Models.Enums;
using Booking.System.ReservationService.DataBase.Models.Enums;

namespace Booking.System.ReservationService.DataBase.Converters.Enums;

public class PaymentStatusConverter
{
    public static PaymentStatus Convert(DbPaymentStatus paymentStatus)
    {
        return paymentStatus switch
        {
            DbPaymentStatus.PAID => PaymentStatus.PAID,
            DbPaymentStatus.CANCELED => PaymentStatus.CANCELED,
            _ => PaymentStatus.PAID
        };
    }
    
    public static DbPaymentStatus Convert(PaymentStatus paymentStatus)
    {
        return paymentStatus switch
        {
            PaymentStatus.PAID => DbPaymentStatus.PAID,
            PaymentStatus.CANCELED => DbPaymentStatus.CANCELED,
            _ => DbPaymentStatus.PAID
        };
    }
}