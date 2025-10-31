using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class HotelDtoWithFullAddress
{
    /// <summary>
    /// Идентификатор отеля.
    /// </summary>
    [Required]
    [DataMember(Name = "hotelUid")]
    [JsonPropertyName("hotelUid")]
    public Guid HotelUid { get; set; }
    
    /// <summary>
    /// Название отеля.
    /// </summary>
    [Required]
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    /// <summary>
    /// Полный адресс.
    /// </summary>
    [Required]
    [DataMember(Name = "fullAddress")]
    [JsonPropertyName("fullAddress")]
    public string FullAddress { get; set; }
    
    /// <summary>
    /// Полный адресс.
    /// </summary>
    [Required]
    [DataMember(Name = "stars")]
    [JsonPropertyName("stars")]
    public int Stars { get; set; }

    public HotelDtoWithFullAddress(Guid hotelUid,
        string name,
        string fullAddress,
        int stars)
    {
        HotelUid = hotelUid;
        Name = name;
        FullAddress = fullAddress;
        Stars = stars;
    }
}