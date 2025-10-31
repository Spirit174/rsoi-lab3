using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.LoyaltyService.DTO.Models;

public class IncreaseBool
{
    /// <summary>
    /// Статус лояльности.
    /// </summary>
    [Required]
    [DataMember(Name = "isIncrease")]
    [JsonPropertyName("isIncrease")]
    public bool IsIncrease { get; set; }

    public IncreaseBool(bool isIncrease)
    {
        IsIncrease = isIncrease;
    }
}