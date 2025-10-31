using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DataBase.Models;

namespace Booking.System.ReservationService.DataBase.Converters;


public class HotelConverter
{
    public static Hotel Convert(DbHotel hotel)
    {
        return new Hotel(hotel.Id,
            hotel.HotelUid,
            hotel.Name,
            hotel.Country,
            hotel.City,
            hotel.Address,
            hotel.Stars,
            hotel.Price);
    }
    
    public static DbHotel Convert(Hotel hotel)
    {
        return new DbHotel(hotel.Id,
            hotel.HotelUid,
            hotel.Name,
            hotel.Country,
            hotel.City,
            hotel.Address,
            hotel.Stars,
            hotel.Price);
    }
}