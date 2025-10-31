using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.PaymentService.DTO.Models;

public class PaymentInfoDto
{
    /// <summary>
    /// Статус оплаты.
    /// </summary>
    [Required]
    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    /// <summary>
    /// Скидка.
    /// </summary>
    [Required]
    [DataMember(Name = "price")]
    [JsonPropertyName("price")]
    public int Price { get; set; }

    public PaymentInfoDto(string status,
        int price)
    {
        Status = status;
        Price = price;
    }
}
