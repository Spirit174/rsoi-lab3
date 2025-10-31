using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.DTO.Converters;
using Booking.System.ReservationService.DTO.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking.System.ReservationService.Controllers;

[ApiController]
[Route("/api/v1")]
public class ReservationController: ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;
    
    public ReservationController(IReservationService reservationService,
        ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Отмена бронирования.
    /// </summary>
    [HttpPost("reservations/{reservationId}")]
    public async Task<ActionResult> CancelReservation([FromRoute] Guid reservationId)
    {
        _logger.LogInformation($"CancelReservation: {reservationId}");
        try
        {
            if (!await _reservationService.CancelReservation(reservationId))
                return NotFound(new ErrorResponse("Нeт такой брони."));

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Получить информацию о бронировании по идентифкатору.
    /// </summary>
    [HttpGet("reservations/{reservationId}")]
    public async Task<ActionResult<ReservationDto>> GetReservationId([FromRoute] Guid reservationId)
    {
        _logger.LogInformation($"GetReservationId: {reservationId}");
        try
        {
            var reservation = await _reservationService.GetReservationByReservationIdAsync(reservationId);
            
            if (reservation is null)
                return NotFound(new ErrorResponse("Нeт такой брони."));

            return Ok(ReservationConverter.Convert(reservation));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Создать бронь.
    /// </summary>
    [HttpPost("reservations")]
    public async Task<ActionResult> CreateReservation([FromBody] CreateReservationDto createReservationDto)
    {
        _logger.LogInformation($"CreateReservation: {createReservationDto}");
        try
        {
            var reservation = await _reservationService.CreateReservationAsync(createReservationDto.ReservationUid, createReservationDto.Username, createReservationDto.PaymentUid,
                createReservationDto.HotelUid, createReservationDto.StartDate, createReservationDto.EndDate);

            return Ok(reservation);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Получить все брони пользователя.
    /// </summary>
    [HttpGet("reservations/user/{userName}")]
    public async Task<ActionResult<List<ReservationDto>>> GetReservationsById([FromRoute] string userName)
    {
        _logger.LogInformation($"GetReservationsById: {userName}");
        try
        {
            var reservations = await _reservationService.GetReservationByUserNameAsync(userName);

            return Ok(reservations.ConvertAll(ReservationConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }
}