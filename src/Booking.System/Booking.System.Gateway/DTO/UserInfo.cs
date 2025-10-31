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

"reservations": [
{
    "reservationUid": "9b4ba1f7-e5ac-465b-ace4-7b54dec20f9a",
    "hotel": {
        "hotelUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
        "name": "Ararat Park Hyatt Moscow",
        "fullAddress": "Россия, Москва, Неглинная ул., 4",
        "stars": 5
    },
    "startDate": "2021-10-08",
    "endDate": "2021-10-11",
    "status": "PAID",
    "payment": {
        "status": "PAID",
        "price": 27000
    }
}
],
"loyalty": {
    "status": "GOLD",
    "discount": 10
}