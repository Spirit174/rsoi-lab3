using System.Text.Json;
using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.DTO.Converters;
using Booking.System.ReservationService.DTO.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking.System.ReservationService.Controllers;

[ApiController]
[Route("/api/v1")]
public class HotelController: ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelController> _logger;
    
    public HotelController(IHotelService hotelService,
        ILogger<HotelController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }
    
    /// <summary>
    /// Получить страницу отелей.
    /// </summary>
    [HttpGet("hotels")]
    public async Task<ActionResult<HotelPagesDto>> GetHotelsPages([FromQuery] int page, [FromQuery] int size)
    {
        _logger.LogInformation($"GetHotelsPages: {page}/{size}");
        try
        {
            var pages = await _hotelService.GetHotelsByPagesAsync(page, size);
            _logger.LogInformation("Returning hotels: {Page}, {Size}, {Total}, {ItemsCount}", 
                pages.Page, pages.Size, pages.TotalElements, pages.Hotels?.Count);
            
            var json = JsonSerializer.Serialize(pages);
            _logger.LogInformation("Serialized JSON: {Json}", json);
            return Ok(HotelPagesDtoConverter.Convert(pages));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Получить отель по идентификатору.
    /// </summary>
    [HttpGet("hotels/{hotelId}")]
    public async Task<ActionResult<HotelDto>> GetHotelById([FromRoute] Guid hotelId)
    {
        _logger.LogInformation($"GetHotelById: {hotelId}");
        try
        {
            var hotel = await _hotelService.GetHotelByHotelIdAsync(hotelId);
            
            if (hotel is null)
                return BadRequest(new ErrorResponse("Нeт такого отеля."));

            return Ok(HotelDtoConverter.Convert(hotel));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
}