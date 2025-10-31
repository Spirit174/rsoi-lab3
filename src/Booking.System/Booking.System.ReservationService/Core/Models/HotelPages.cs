namespace Booking.System.ReservationService.Core.Models;

public class HotelPages
{
    public List<Hotel> Hotels { get; set; }
    
    public int Page { get; set; }
    
    public int Size { get; set; }
    
    public int TotalElements { get; set; }

    public HotelPages(List<Hotel> hotels,
        int page,
        int size,
        int totalElements)
    {
        Hotels = hotels;
        Page = page;
        Size = size;
        TotalElements = totalElements;
    }
}