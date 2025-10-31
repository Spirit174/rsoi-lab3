using System.ComponentModel.DataAnnotations.Schema;
using Booking.System.ReservationService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.ReservationService.DataBase.Models;

public class DbReservation
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public Guid ReservationUid { get; set; }
    
    public string Username { get; set; }
    
    public Guid PaymentUid { get; set; }
    
    public int HotelId { get; set; }
    
    public DbPaymentStatus Status { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    [ForeignKey("HotelId")]
    public virtual DbHotel Hotel { get; set; } = null!;

    public DbReservation(Guid reservationUid,
        string username,
        Guid paymentUid,
        int hotelId,
        DbPaymentStatus status,
        DateTime startDate,
        DateTime endDate)
    {
        ReservationUid = reservationUid;
        Username = username;
        Status = status;
        PaymentUid = paymentUid;
        HotelId = hotelId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
    }
}