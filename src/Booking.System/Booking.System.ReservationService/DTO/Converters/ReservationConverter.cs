using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DTO.Converters.Enums;
using Booking.System.ReservationService.DTO.Models;

namespace Booking.System.ReservationService.DTO.Converters;

public class ReservationConverter
{
    public static ReservationDto? Convert(Reservation? reservation)
    {
        if (reservation == null)
            return null;
        return new ReservationDto(reservation.ReservationUid,
            reservation.HotelUid,
            reservation.PaymentUid,
            reservation.StartDate,
            reservation.EndDate,
            ReservationStatusConverter.Convert(reservation.Status));
    }
}