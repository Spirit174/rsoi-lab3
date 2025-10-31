using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.PaymentService.DTO.Models;

public class ErrorResponse
{
    /// <summary>
    /// Описание ошибки.
    /// </summary>
    [Required]
    [DataMember(Name = "message")]
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ErrorResponse(string message)
    {
        Message = message;
    }
}