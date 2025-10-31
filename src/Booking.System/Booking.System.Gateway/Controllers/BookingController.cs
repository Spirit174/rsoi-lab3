using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Booking.System.Gateway.ApiClients;
using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;
using Booking.System.LoyaltyService.DTO.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Booking.System.Gateway.Controllers;

[ApiController]
[Route("/api/v1")]
public class BookingController: ControllerBase
{
    private readonly ILogger<BookingController> _logger;
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IPaymentClient _paymentClient;
    private readonly IReservationClient _reservationClient;

    public BookingController(ILogger<BookingController> logger,
        ILoyaltyClient loyaltyClient,
        IPaymentClient paymentClient,
        IReservationClient reservationClient)
    {
        _logger = logger;
        _loyaltyClient = loyaltyClient;
        _paymentClient = paymentClient;
        _reservationClient = reservationClient;
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
        try
        {
            var pages = await _reservationClient.GetHotelsPageAsync(page, size);
            
            return Ok(pages);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
        try
        {
            var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
            if (string.IsNullOrEmpty(username))
                return BadRequest("X-User-Name header is required");
        
            var reservations = await _reservationClient.GetReservationByUsername(username);
            var loyalty = await _loyaltyClient.GetLoyaltyAsync(username);
        
            var list = new List<ReservationDtoWithHotelAndPayment>();

            foreach (var reservation in reservations)
            {
                var hotel = await _reservationClient.GetHotelByIdAsync(reservation.HotelUid);

                var fullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address;

                var payment = await _paymentClient.GetPaymentAsync(reservation.PaymentUid);
            
                var hotelDtoWithFullAddress = new HotelDtoWithFullAddress(hotel.HotelUid, hotel.Name, fullAddress, hotel.Stars);
                var reser = new ReservationDtoWithHotelAndPayment(reservation.ReservationUid, hotelDtoWithFullAddress,
                    DateOnly.FromDateTime(reservation.StartDate), DateOnly.FromDateTime(reservation.EndDate), reservation.Status, payment);
            
                list.Add(reser);
            }
        
            var userInfo = new UserInfoDto(list, loyalty);

            var json = JsonSerializer.Serialize(userInfo);
            _logger.LogInformation("Serialized JSON: {Json}", json);
            return Ok(userInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
        try
        {
            var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
            if (string.IsNullOrEmpty(username))
                return BadRequest("X-User-Name header is required");
            
            var reservations = await _reservationClient.GetReservationByUsername(username);
            
            if (reservations.Count == 0)
                return Ok(new List<ReservationDtoWithHotelAndPayment>());
            
            var list = new List<ReservationDtoWithHotelAndPayment>();

            foreach (var reservation in reservations)
            {
                var hotel = await _reservationClient.GetHotelByIdAsync(reservation.HotelUid);

                var fullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address;

                var payment = await _paymentClient.GetPaymentAsync(reservation.PaymentUid);
            
                var hotelDtoWithFullAddress = new HotelDtoWithFullAddress(hotel.HotelUid, hotel.Name, fullAddress, hotel.Stars);
                var reser = new ReservationDtoWithHotelAndPayment(reservation.ReservationUid, hotelDtoWithFullAddress,
                    DateOnly.FromDateTime(reservation.StartDate), DateOnly.FromDateTime(reservation.EndDate), reservation.Status, payment);
            
                list.Add(reser);
            }
            
            var json = JsonSerializer.Serialize(list);
            _logger.LogInformation("Serialized JSON: {Json}", json);
            return Ok(list);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
        try
        {
            var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
            if (string.IsNullOrEmpty(username))
                return BadRequest("X-User-Name header is required");
            
            var reservation = await _reservationClient.GetReservationById(reservationUid);
            
            var hotel = await _reservationClient.GetHotelByIdAsync(reservation.HotelUid);

            var fullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address;

            var payment = await _paymentClient.GetPaymentAsync(reservation.PaymentUid);

            var hotelDtoWithFullAddress =
                new HotelDtoWithFullAddress(hotel.HotelUid, hotel.Name, fullAddress, hotel.Stars);
            var res = new ReservationDtoWithHotelAndPayment(reservation.ReservationUid, hotelDtoWithFullAddress,
                DateOnly.FromDateTime(reservation.StartDate), DateOnly.FromDateTime(reservation.EndDate), reservation.Status, payment);
            
            var json = JsonSerializer.Serialize(res);
            _logger.LogInformation("Serialized JSON: {Json}", json);
            return Ok(res);
        }
        catch (HotelNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(404, e.Message);
        }
        catch (PaymentNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(404, e.Message);
        }
        catch (ReservationNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(404, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
    [SwaggerResponse(statusCode: 201, description: "Бронирование успешно создано.")]
    [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponse), description: "Отсутствует заголовок или невалидные данные запроса.")]
    [SwaggerResponse(statusCode: 500, type: typeof(ErrorResponse), description: "Ошибка на стороне сервера.")]
    public async Task<ActionResult<CreateReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
    {
        try
        {
            var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
            if (string.IsNullOrEmpty(username))
                return BadRequest("X-User-Name header is required");

            var reservationUid = Guid.NewGuid();
            
            var hotel = await _reservationClient.GetHotelByIdAsync(request.HotelUid);
            
            var difference = request.EndDate - request.StartDate;
            var countDays = difference.Days;
            var loyalty = await _loyaltyClient.GetLoyaltyAsync(username);
            var price = countDays * hotel.Price * (100 - loyalty.Discount) / 100;
            _logger.LogInformation($"!!! {difference}, {countDays}, {hotel.Price}, {loyalty.Discount}");
            
            var paymentUid = await _paymentClient.CreatePaymentAsync(price);
            var payment = await _paymentClient.GetPaymentAsync(paymentUid);
            
            await _reservationClient.CreateReservation(new CreateReservationDto(reservationUid, username, paymentUid, request.HotelUid, request.StartDate, request.EndDate));
            await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, true);

            var reservationResponse = new CreateReservationResponse(reservationUid, request.HotelUid, DateOnly.FromDateTime(request.StartDate), DateOnly.FromDateTime(request.EndDate),
                loyalty.Discount, payment.Status, payment);
            
            return StatusCode(200, reservationResponse);
        }
        catch (HotelNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (PaymentNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (ReservationNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
        try
        {
            var username = Request.Headers["X-User-Name"].FirstOrDefault();
        
            if (string.IsNullOrEmpty(username))
                return BadRequest("X-User-Name header is required");

            await _reservationClient.CancelReservation(reservationUid); 
            var reserver = await _reservationClient.GetReservationById(reservationUid);
            await _paymentClient.UpdatePaymentAsync(reserver.PaymentUid);
            await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, false);
            
            return StatusCode(204);
        }
        catch (HotelNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (PaymentNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (ReservationNotFoundException e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(400, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in reservation service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
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
        
        try
        {
            var loyaltyInfo = await _loyaltyClient.GetLoyaltyAsync(username);
            return Ok(loyaltyInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in loyalty service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }
    
}