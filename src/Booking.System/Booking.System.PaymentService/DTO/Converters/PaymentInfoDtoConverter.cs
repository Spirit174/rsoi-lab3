using Booking.System.PaymentService.Core.Models;
using Booking.System.PaymentService.DTO.Converters.Enums;
using Booking.System.PaymentService.DTO.Models;

namespace Booking.System.PaymentService.DTO.Converters;


public class PaymentInfoDtoConverter
{
    public static PaymentInfoDto Convert(Payment payment)
    {
        return new PaymentInfoDto(PaymentInfoDtoStatusConverter.Convert(payment.PaymentStatus),
            payment.Price);
    }
}