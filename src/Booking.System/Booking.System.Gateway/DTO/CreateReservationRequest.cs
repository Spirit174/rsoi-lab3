using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class CreateReservationRequest
{
    /// <summary>
    /// Идентификатор отеля.
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
    /// Конец брони.
    /// </summary>
    [Required]
    [DataMember(Name = "endDate")]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    public CreateReservationRequest(Guid hotelUid,
        DateTime startDate,
        DateTime endDate)
    {
        HotelUid = hotelUid;
        StartDate = startDate;
        EndDate = endDate;
    }
}
