using Booking.System.PaymentService.Core.Models.Enums;

namespace Booking.System.PaymentService.Core.Models;

public class Payment
{
    public int Id { get; set; }
    
    public Guid PaymentUid { get; set; }
    
    public PaymentStatus PaymentStatus { get; set; }
    
    public int Price { get; set; }

    public Payment(int id,
        Guid paymentUid,
        PaymentStatus paymentStatus,
        int price)
    {
        Id = id;
        PaymentUid = paymentUid;
        PaymentStatus = paymentStatus;
        Price = price;
    } 
}