using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.PaymentService.DTO.Models;

public class PaymentIdDto
{
    /// <summary>
    /// Идентификатор платежа.
    /// </summary>
    [Required]
    [DataMember(Name = "paymentId")]
    [JsonPropertyName("paymentId")]
    public Guid PaymentId { get; set; }

    public PaymentIdDto(Guid paymentId)
    {
        PaymentId = paymentId;
    }
}