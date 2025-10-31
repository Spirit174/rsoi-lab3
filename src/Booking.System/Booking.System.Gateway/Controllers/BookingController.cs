using System.Text.Json;
using Booking.System.Gateway.ApiClients;
using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;
using Booking.System.Gateway.Services;
using Booking.System.LoyaltyService.DTO.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Booking.System.Gateway.Controllers;

[ApiController]
[Route("/api/v1")]
public class BookingController: ControllerBase
{
    private readonly ILogger<BookingController> _logger;
    private readonly IGatewayService _gatewayService;

    public BookingController(ILogger<BookingController> logger, IGatewayService gatewayService)
    {
        _logger = logger;
        _gatewayService = gatewayService;
    }

    /// <summary>
    /// Получить список отелей.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="size">Размер страницы.</param>
    /// <response code="200">Список отелей успешно получен.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("hotels")]
    [SwaggerOperation("Метод для получения списка отелей.", "Метод для получения списка отелей.")]
    [SwaggerResponse(statusCode: 200, description: "Список отелей успешно получен.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<HotelPagesDto>> GetHotels([FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var response = await _gatewayService.GetHotelsAsync(page, size);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }
        
        return Ok(response.Response);
    }
    
    /// <summary>
    /// Получить информацию о пользователе.
    /// </summary>
    /// <response code="200">Информация о пользователе успешно получена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("me")]
    [SwaggerOperation("Метод для получения информации о пользователе.", "Метод для получения информации о пользователе.")]
    [SwaggerResponse(statusCode: 200, description: "Информация о пользователе успешно получена.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.GetUserInfoAsync(username);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        var json = JsonSerializer.Serialize(response.Response);
        _logger.LogInformation("Serialized JSON: {Json}", json);
        return Ok(response.Response);
    }
    
    /// <summary>
    /// Получить информацию по всем бронированиям пользователя.
    /// </summary>
    /// <response code="200">Список бронирований успешно получен.</response>
    /// <response code="400">Отсутствует заголовок.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("reservations")]
    [SwaggerOperation("Метод для получения информации о всех бронированиях пользователя.", "Метод для получения информации о всех бронированиях пользователя.")]
    [SwaggerResponse(statusCode: 200, description: "Список бронирований успешно получен.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<List<ReservationDtoWithHotelAndPayment>>> GetUserReservations()
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.GetUserReservationsAsync(username);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        var json = JsonSerializer.Serialize(response.Response);
        _logger.LogInformation("Serialized JSON: {Json}", json);
        return Ok(response.Response);
    }

    /// <summary>
    /// Получить информацию по конкретному бронированию.
    /// </summary>
    /// <param name="reservationUid">Id бронирования.</param>
    /// <response code="200">Информация о бронировании успешно получена.</response>
    /// <response code="400">Отсутствует заголовок.</response>
    /// <response code="404">Бронирование не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("reservations/{reservationUid}")]
    [SwaggerOperation("Метод для получения информации о конкретном бронирование пользователя.", "Метод для получения информации о конкретном бронирование пользователя.")]
    [SwaggerResponse(statusCode: 200, description: "Информация о бронировании успешно получена.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок.")]
    [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponse), description: "Бронирование не найдено.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<ReservationDtoWithHotelAndPayment>> GetReservation([FromRoute] Guid reservationUid)
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.GetReservationAsync(username, reservationUid);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        if (response.Response == null)
        {
            return NotFound(new ErrorResponse("Бронирование не найдено"));
        }

        var json = JsonSerializer.Serialize(response.Response);
        _logger.LogInformation("Serialized JSON: {Json}", json);
        return Ok(response.Response);
    }
    
    /// <summary>
    /// Забронировать отель.
    /// </summary>
    /// <param name="request">Данные для бронирования отеля.</param>
    /// <response code="200">Бронирование успешно создано.</response>
    /// <response code="400">Отсутствует заголовок или невалидные данные запроса.</response>
    /// <response code="404">Отель не найден.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost("reservations")]
    [SwaggerOperation("Метод для бронирования отеля.", "Метод для бронирования отеля.")]
    [SwaggerResponse(statusCode: 200, description: "Бронирование успешно создано.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок или невалидные данные запроса.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<CreateReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.CreateReservationAsync(username, request);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        if (response.Response == null)
        {
            return StatusCode(500, new ErrorResponse("Ошибка при создании бронирования"));
        }

        return StatusCode(200, response.Response);
    }
    
    /// <summary>
    /// Отменить бронирование.
    /// </summary>
    /// <param name="reservationUid">UID бронирования для отмены.</param>
    /// <response code="204">Бронирование успешно отменено.</response>
    /// <response code="400">Отсутствует заголовок.</response>
    /// <response code="404">Бронирование не найдено.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpDelete("reservations/{reservationUid}")]
    [SwaggerOperation("Метод для отмены бронирования отеля.", "Метод для отмены бронирования отеля.")]
    [SwaggerResponse(statusCode: 204, description: "Бронирование успешно отменено.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок.")]
    [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponse), description: "Бронирование не найдено.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<IActionResult> CancelReservation(Guid reservationUid)
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.CancelReservationAsync(username, reservationUid);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        return StatusCode(204);
    }
    
    /// <summary>
    /// Получить информацию о статусе в программе лояльности.
    /// </summary>
    /// <response code="200">Информация о статусе лояльности успешно получена.</response>
    /// <response code="400">Отсутствует заголовок.</response>
    /// <response code="404">Информация о программе лояльности не найдена.</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpGet("loyalty")]
    [SwaggerOperation("Метод для получения статуса лояльности.", "Метод для получения статуса лояльности.")]
    [SwaggerResponse(statusCode: 200, description: "Статус лояльности успешно получен.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<LoyaltyInfoDto>> GetLoyaltyInfo()
    {
        var username = Request.Headers["X-User-Name"].FirstOrDefault();
        if (string.IsNullOrEmpty(username))
            return BadRequest("X-User-Name header is required");

        var response = await _gatewayService.GetLoyaltyInfoAsync(username);
        
        if (!response.IsSuccess)
        {
            return StatusCode(response.StatusCode, new ErrorResponse(response.GetErrorMessage()));
        }

        return Ok(response.Response);
    }
    
    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }
}