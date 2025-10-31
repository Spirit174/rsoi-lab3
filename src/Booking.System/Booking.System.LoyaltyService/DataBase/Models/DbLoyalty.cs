using System.ComponentModel.DataAnnotations.Schema;
using Booking.System.LoyaltyService.DataBase.Models.Enums;

namespace Booking.System.LoyaltyService.DataBase.Models;

public class DbLoyalty
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public int ReservationCount { get; set; }
    
    public LoyaltyStatus Status { get; set; }
    
    public int Discount { get; set; }

    public DbLoyalty(int id,
        string username,
        int reservationCount,
        LoyaltyStatus status,
        int discount)
    {
        Id = id;
        Username = username;
        ReservationCount = reservationCount;
        Status = status;
        Discount = discount;
    }
}

