using Booking.System.PaymentService.Core.Models.Enums;
using Booking.System.PaymentService.DataBase.Models.Enums;

namespace Booking.System.PaymentService.DataBase.Converters.Enums;

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