using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class HotelDto
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
    /// Название cтраны в которой отель.
    /// </summary>
    [Required]
    [DataMember(Name = "country")]
    [JsonPropertyName("country")]
    public string Country { get; set; }
    
    /// <summary>
    /// Название города в которой отель.
    /// </summary>
    [Required]
    [DataMember(Name = "city")]
    [JsonPropertyName("city")]
    public string City { get; set; }
    
    /// <summary>
    /// Адресс отеля.
    /// </summary>
    [Required]
    [DataMember(Name = "address")]
    [JsonPropertyName("address")]
    public string Address { get; set; }
    
    /// <summary>
    /// Количество звезд отеля.
    /// </summary>
    [Required]
    [DataMember(Name = "stars")]
    [JsonPropertyName("stars")]
    public int Stars { get; set; }
    
    /// <summary>
    /// Цена отеля за одну ночь.
    /// </summary>
    [Required]
    [DataMember(Name = "price")]
    [JsonPropertyName("price")]
    public int Price { get; set; }
    
    public HotelDto(Guid hotelUid,
        string name,
        string country,
        string city,
        string address,
        int stars,
        int price)
    {
        HotelUid = hotelUid;
        Name = name;
        Country = country;
        City = city;
        Address = address;
        Stars = stars;
        Price = price;
    }
}