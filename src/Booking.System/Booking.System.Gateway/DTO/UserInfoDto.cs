using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class UserInfoDto
{
    /// <summary>
    /// Список броней.
    /// </summary>
    [Required]
    [DataMember(Name = "reservations")]
    [JsonPropertyName("reservations")]
    public List<ReservationDtoWithHotelAndPayment> Reservations { get; set; }
    
    /// <summary>
    /// Информация о скидке.
    /// </summary>
    [Required]
    [DataMember(Name = "loyalty")]
    [JsonPropertyName("loyalty")]
    public object? Loyalty  { get; set; }
    
    public UserInfoDto(List<ReservationDtoWithHotelAndPayment> reservations,
        object? loyalty)
    {
        Reservations = reservations;
        Loyalty = loyalty;
    }
}