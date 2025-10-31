using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DataBase.Converters.Enums;
using Booking.System.ReservationService.DataBase.Models;

namespace Booking.System.ReservationService.DataBase.Converters;


public class ReservationConverter
{
    public static Reservation Convert(DbReservation reservation, Guid dbHotelId)
    {
        return new Reservation(reservation.Id,
            reservation.ReservationUid,
            reservation.Username,
            reservation.PaymentUid,
            dbHotelId,
            PaymentStatusConverter.Convert(reservation.Status),
            reservation.StartDate,
            reservation.EndDate);
    }
    
    public static DbReservation Convert(Reservation reservation, DbHotel hotel)
    {
        return new DbReservation(reservation.ReservationUid,
            reservation.Username,
            reservation.PaymentUid,
            hotel.Id,
            PaymentStatusConverter.Convert(reservation.Status),
            reservation.StartDate,
            reservation.EndDate);
    }
}