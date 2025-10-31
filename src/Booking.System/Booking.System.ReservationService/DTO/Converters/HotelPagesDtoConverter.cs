using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DTO.Models;

namespace Booking.System.ReservationService.DTO.Converters;

public class HotelPagesDtoConverter
{
    public static HotelPagesDto Convert(HotelPages hotelPages)
    {
        return new HotelPagesDto(hotelPages.Page,
            hotelPages.Size,
            hotelPages.TotalElements,
            hotelPages.Hotels.ConvertAll(HotelDtoConverter.Convert));
    }
}