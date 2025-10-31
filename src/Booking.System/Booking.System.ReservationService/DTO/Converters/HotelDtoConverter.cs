using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DTO.Models;

namespace Booking.System.ReservationService.DTO.Converters;

public class HotelDtoConverter
{
    public static HotelDto Convert(Hotel hotel)
    {
        return new HotelDto(hotel.HotelUid,
            hotel.Name,
            hotel.Country,
            hotel.City,
            hotel.Address,
            hotel.Stars,
            hotel.Price);
    }
}