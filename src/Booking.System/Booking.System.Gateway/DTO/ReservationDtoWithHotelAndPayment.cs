using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class ReservationDtoWithHotelAndPayment
{
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    [Required]
    [DataMember(Name = "reservationUid")]
    [JsonPropertyName("reservationUid")]
    public Guid ReservationUid { get; set; }
    
    /// <summary>
    /// Отель.
    /// </summary>
    [Required]
    [DataMember(Name = "hotel")]
    [JsonPropertyName("hotel")]
    public HotelDtoWithFullAddress Hotel { get; set; }
    
    /// <summary>
    /// Начало брони.
    /// </summary>
    [Required]
    [DataMember(Name = "startDate")]
    [JsonPropertyName("startDate")]
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Конец брони.
    /// </summary>
    [Required]
    [DataMember(Name = "endDate")]
    [JsonPropertyName("endDate")]
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Статус брони.
    /// </summary>
    [Required]
    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    /// <summary>
    /// Платеж.
    /// </summary>
    [Required]
    [DataMember(Name = "payment")]
    [JsonPropertyName("payment")]
    public PaymentInfoDto Payment { get; set; }
    
    public ReservationDtoWithHotelAndPayment(Guid reservationUid,
        HotelDtoWithFullAddress hotelDtoWithFullAddress,
        DateOnly startDate,
        DateOnly endDate,
        string status,
        PaymentInfoDto payment)
    {
        ReservationUid = reservationUid;
        Hotel = hotelDtoWithFullAddress;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        Payment = payment;
    }
}

