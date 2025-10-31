using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class CreateReservationDto
{
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    [Required]
    [DataMember(Name = "reservationUid")]
    [JsonPropertyName("reservationUid")]
    public Guid ReservationUid { get; set; }
     
    /// <summary>
    /// Имя.
    /// </summary>
    [Required]
    [DataMember(Name = "userName")]
    [JsonPropertyName("userName")]
    public string Username { get; set; }
    
    /// <summary>
    /// Идентификатор платежа.
    /// </summary>
    [Required]
    [DataMember(Name = "paymentUid")]
    [JsonPropertyName("paymentUid")]
    public Guid PaymentUid { get; set; }
    
    /// <summary>
    /// Идентификатор брони.
    /// </summary>
    [Required]
    [DataMember(Name = "hotelUid")]
    [JsonPropertyName("hotelUid")]
    public Guid HotelUid { get; set; }
    
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

    public CreateReservationDto(Guid reservationUid,
        string username,
        Guid paymentUid,
        Guid hotelUid,
        DateTime startDate,
        DateTime endDate)
    {
        ReservationUid = reservationUid;
        Username = username;
        PaymentUid = paymentUid;
        HotelUid = hotelUid;
        StartDate = startDate;
        EndDate = endDate;
    }
}
