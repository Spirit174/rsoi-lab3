using Booking.System.ReservationService.Core.Models.Enums;

namespace Booking.System.ReservationService.Core.Models;

public class Reservation
{
    public int Id { get; set; }
    
    public Guid ReservationUid { get; set; }
    
    public string Username { get; set; }
    
    public Guid PaymentUid { get; set; }
    
    public Guid HotelUid { get; set; }
    
    public PaymentStatus Status { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }

    public Reservation(int id,
        Guid reservationUid,
        string username,
        Guid paymentUid,
        Guid hotelUid,
        PaymentStatus status,
        DateTime startDate,
        DateTime endDate)
    {
        Id = id;
        ReservationUid = reservationUid;
        Username = username;
        Status = status;
        PaymentUid = paymentUid;
        HotelUid = hotelUid;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
    }
}