namespace Booking.System.Gateway.Exceptions;

public class HotelNotFoundException: Exception
{
    public HotelNotFoundException() : base()
    {

    }
    
    public HotelNotFoundException(string? message) : base(message)
    {

    }
    
    public HotelNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}