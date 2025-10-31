using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class LoyaltyInfoDto
{
    /// <summary>
    /// Статус лояльности.
    /// </summary>
    [Required]
    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    /// <summary>
    /// Скидка.
    /// </summary>
    [Required]
    [DataMember(Name = "discount")]
    [JsonPropertyName("discount")]
    public int Discount { get; set; }
    
    /// <summary>
    /// Количество бронирований.
    /// </summary>
    [Required]
    [DataMember(Name = "reservationCount")]
    [JsonPropertyName("reservationCount")]
    public int ReservationCount { get; set; }

    public LoyaltyInfoDto(string status,
        int discount,
        int reservationCount)
    {
        Status = status;
        Discount = discount;
        ReservationCount = reservationCount;
    }
}