using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.ReservationService.DTO.Models;

public class ReservationDto
{
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    [Required]
    [DataMember(Name = "reservationUid")]
    [JsonPropertyName("reservationUid")]
    public Guid ReservationUid { get; set; }
    
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    [Required]
    [DataMember(Name = "hotelUid")]
    [JsonPropertyName("hotelUid")]
    public Guid HotelUid { get; set; }
    
    /// <summary>
    /// Идентификатор платежа.
    /// </summary>
    [Required]
    [DataMember(Name = "paymentUid")]
    [JsonPropertyName("paymentUid")]
    public Guid PaymentUid { get; set; }
    
    /// <summary>
    /// Начало бррони.
    /// </summary>
    [Required]
    [DataMember(Name = "startDate")]
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Конец бррони.
    /// </summary>
    [Required]
    [DataMember(Name = "endDate")]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Статус брони.
    /// </summary>
    [Required]
    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    

    public ReservationDto(Guid reservationUid,
        Guid hotelUid,
        Guid paymentUid,
        DateTime startDate,
        DateTime endDate,
        string status)
    {
        ReservationUid = reservationUid;
        HotelUid = hotelUid;
        PaymentUid = paymentUid;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }
}
