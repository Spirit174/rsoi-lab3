namespace Booking.System.Gateway.Exceptions;

public class ReservationNotFoundException: Exception
{
    public ReservationNotFoundException() : base()
    {

    }
    
    public ReservationNotFoundException(string? message) : base(message)
    {

    }
    
    public ReservationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}