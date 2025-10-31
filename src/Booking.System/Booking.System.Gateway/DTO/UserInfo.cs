using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class UserInfo
{
    /// <summary>
    /// Список броней.
    /// </summary>
    [Required]
    [DataMember(Name = "hotel")]
    [JsonPropertyName("hotel")]
    public List<ReservationDtoWithHotelAndPayment> Hotel { get; set; }
    
    public UserInfo()
    {
        
    }
}

