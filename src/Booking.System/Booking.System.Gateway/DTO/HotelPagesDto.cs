using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Booking.System.Gateway.DTO;

public class HotelPagesDto
{
    /// <summary>
    /// Страница.
    /// </summary>
    [Required]
    [DataMember(Name = "page")]
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    /// <summary>
    /// Размер страницы.
    /// </summary>
    [Required]
    [DataMember(Name = "pageSize")]
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    [Required]
    [DataMember(Name = "totalElements")]
    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }
    
    /// <summary>
    /// Список отелей.
    /// </summary>
    [Required]
    [DataMember(Name = "items")]
    [JsonPropertyName("items")]
    public List<HotelDto> Items { get; set; }

    public HotelPagesDto(int page, 
        int pageSize,
        int totalElements,
        List<HotelDto> items)
    {
        Page = page;
        PageSize = pageSize;
        TotalElements = totalElements;
        Items = items;
    }
}