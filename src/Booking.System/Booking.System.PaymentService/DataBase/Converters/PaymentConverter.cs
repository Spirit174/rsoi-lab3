using Booking.System.PaymentService.Core.Models;
using Booking.System.PaymentService.DataBase.Converters.Enums;
using Booking.System.PaymentService.DataBase.Models;

namespace Booking.System.PaymentService.DataBase.Converters;


public class PaymentConverter
{
    public static Payment Convert(DbPayment payment)
    {
        return new Payment(payment.Id,
            payment.PaymentUid,
            PaymentStatusConverter.Convert(payment.PaymentStatus),
            payment.Price);
    }
    
    public static DbPayment Convert(Payment payment)
    {
        return new DbPayment(payment.PaymentUid,
            PaymentStatusConverter.Convert(payment.PaymentStatus),
            payment.Price);
    }
}