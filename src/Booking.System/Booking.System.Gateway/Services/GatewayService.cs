using Booking.System.Gateway.ApiClients;
using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;

namespace Booking.System.Gateway.Services;

public class GatewayService : IGatewayService
{
    private readonly ILogger<GatewayService> _logger;
    private readonly ILoyaltyClient _loyaltyClient;
    private readonly IPaymentClient _paymentClient;
    private readonly IReservationClient _reservationClient;
    private readonly IRetryQueue _retryQueue;

    public GatewayService(
        ILogger<GatewayService> logger,
        ILoyaltyClient loyaltyClient,
        IPaymentClient paymentClient,
        IReservationClient reservationClient,
        IRetryQueue retryQueue)
    {
        _logger = logger;
        _loyaltyClient = loyaltyClient;
        _paymentClient = paymentClient;
        _reservationClient = reservationClient;
        _retryQueue = retryQueue;
    }

    public async Task<ServiceResponse<HotelPagesDto>> GetHotelsAsync(int page, int size)
    {
        try
        {
            var hotels = await _reservationClient.GetHotelsPageAsync(page, size);
            return ServiceResponse<HotelPagesDto>.Success(hotels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hotels page {Page} size {Size}", page, size);
            return ServiceResponse<HotelPagesDto>.ErrorResponse("Ошибка при получении списка отелей", 500);
        }
    }

    public async Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync(string username)
    {
        try
        {
            // 1. Получаем бронирования - критичный сервис
            var reservationsResponse = await _reservationClient.GetReservationByUsername(username);
            if (reservationsResponse == null || reservationsResponse.Count == 0)
            {
                // Если нет бронирований, возвращаем пустой список с информацией о лояльности
                var loyaltyResponse = await GetLoyaltyWithFallback(username);
                return ServiceResponse<UserInfoDto>.Success(new UserInfoDto(
                    new List<ReservationDtoWithHotelAndPayment>(), 
                    loyaltyResponse
                ));
            }

            var reservationsWithDetails = new List<ReservationDtoWithHotelAndPayment>();

            foreach (var reservation in reservationsResponse)
            {
                var reservationDetail = await GetReservationDetailsAsync(reservation);
                if (reservationDetail != null)
                {
                    reservationsWithDetails.Add(reservationDetail);
                }
            }

            // 2. Получаем информацию о лояльности с fallback
            var loyaltyInfo = await GetLoyaltyWithFallback(username);

            var userInfo = new UserInfoDto(reservationsWithDetails, loyaltyInfo);
            return ServiceResponse<UserInfoDto>.Success(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for {Username}", username);
            return ServiceResponse<UserInfoDto>.ErrorResponse("Ошибка при получении информации о пользователе", 500);
        }
    }

    public async Task<ServiceResponse<List<ReservationDtoWithHotelAndPayment>>> GetUserReservationsAsync(string username)
    {
        try
        {
            var reservationsResponse = await _reservationClient.GetReservationByUsername(username);
            if (reservationsResponse == null || reservationsResponse.Count == 0)
            {
                return ServiceResponse<List<ReservationDtoWithHotelAndPayment>>.Success(
                    new List<ReservationDtoWithHotelAndPayment>());
            }

            var result = new List<ReservationDtoWithHotelAndPayment>();

            foreach (var reservation in reservationsResponse)
            {
                var reservationDetail = await GetReservationDetailsAsync(reservation);
                if (reservationDetail != null)
                {
                    result.Add(reservationDetail);
                }
            }

            return ServiceResponse<List<ReservationDtoWithHotelAndPayment>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for {Username}", username);
            return ServiceResponse<List<ReservationDtoWithHotelAndPayment>>.ErrorResponse(
                "Ошибка при получении списка бронирований", 500);
        }
    }

    public async Task<ServiceResponse<ReservationDtoWithHotelAndPayment?>> GetReservationAsync(string username, Guid reservationUid)
    {
        try
        {
            var reservation = await _reservationClient.GetReservationById(reservationUid);
            if (reservation == null)
            {
                return ServiceResponse<ReservationDtoWithHotelAndPayment?>.ErrorResponse(
                    "Бронирование не найдено", 404);
            }
            

            var reservationDetail = await GetReservationDetailsAsync(reservation);
            return ServiceResponse<ReservationDtoWithHotelAndPayment?>.Success(reservationDetail);
        }
        catch (ReservationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Reservation {ReservationUid} not found for user {Username}", reservationUid, username);
            return ServiceResponse<ReservationDtoWithHotelAndPayment?>.ErrorResponse(
                "Бронирование не найдено", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation {ReservationUid} for user {Username}", 
                reservationUid, username);
            return ServiceResponse<ReservationDtoWithHotelAndPayment?>.ErrorResponse(
                "Ошибка при получении информации о бронировании", 500);
        }
    }

    public async Task<ServiceResponse<CreateReservationResponse?>> CreateReservationAsync(
        string username, CreateReservationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting reservation creation for user: {Username}, hotel: {HotelUid}", 
                username, request.HotelUid);

            // 1. Получаем информацию об отеле - критичный
            var hotel = await _reservationClient.GetHotelByIdAsync(request.HotelUid);
            if (hotel == null)
            {
                return ServiceResponse<CreateReservationResponse?>.ErrorResponse("Отель не найден", 404);
            }

            // 2. Получаем информацию о лояльности с fallback
            var loyaltyResponse = await _loyaltyClient.GetLoyaltyAsync(username);
            var loyaltyInfo = loyaltyResponse ?? new LoyaltyInfoDto("UNKNOWN",0,0) { Discount = 0, ReservationCount = 0 };

            // 3. Рассчитываем стоимость
            var (totalPrice, countDays) = CalculateReservationPrice(request, hotel, loyaltyInfo);

            // 4. Создаем платеж - критичный
            var paymentUid = await _paymentClient.CreatePaymentAsync(totalPrice);
            var payment = await _paymentClient.GetPaymentAsync(paymentUid);

            // 5. Создаем бронирование - критичный
            var reservationUid = Guid.NewGuid();
            await _reservationClient.CreateReservation(new CreateReservationDto(
                reservationUid, username, paymentUid, request.HotelUid, request.StartDate, request.EndDate));

            // 6. Обновляем счетчик бронирований - не критичный, идет в retry queue при ошибке
            try
            {
                await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, true);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Loyalty service unavailable for reservation creation, adding to retry queue");
                
                _retryQueue.Enqueue(new RetryItem
                {
                    OperationType = "UpdateLoyaltyAfterReservation",
                    Username = username,
                    Data = new { Increment = true },
                    Action = async () =>
                    {
                        try
                        {
                            await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, true);
                            return true;
                        }
                        catch (Exception exception)
                        {
                            return false;
                        }
                    }
                });
            }

            var response = new CreateReservationResponse(
                reservationUid,
                request.HotelUid,
                DateOnly.FromDateTime(request.StartDate),
                DateOnly.FromDateTime(request.EndDate),
                loyaltyInfo.Discount,
                payment.Status,
                payment
            );

            _logger.LogInformation("Reservation created successfully: {ReservationUid}", reservationUid);
            return ServiceResponse<CreateReservationResponse?>.Success(response);
        }
        catch (HotelNotFoundException ex)
        {
            _logger.LogWarning(ex, "Hotel {HotelUid} not found for reservation creation", request.HotelUid);
            return ServiceResponse<CreateReservationResponse?>.ErrorResponse("Отель не найден", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation for user {Username}", username);
            return ServiceResponse<CreateReservationResponse?>.ErrorResponse(
                "Ошибка при создании бронирования", 500);
        }
    }

    public async Task<ServiceResponse<bool>> CancelReservationAsync(string username, Guid reservationUid)
    {
        try
        {
            // 1. Получаем бронирование для проверки принадлежности
            var reservation = await _reservationClient.GetReservationById(reservationUid);
            if (reservation == null)
            {
                return ServiceResponse<bool>.ErrorResponse("Бронирование не найдено", 404);
            }

            // 2. Отменяем бронирование - критичный
            await _reservationClient.CancelReservation(reservationUid);

            // 3. Обновляем платеж - критичный
            await _paymentClient.UpdatePaymentAsync(reservation.PaymentUid);

            // 4. Обновляем счетчик лояльности - не критичный, идет в retry queue при ошибке
            try
            {
                await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, false);
            }
            catch(Exception)
            {
                _logger.LogWarning("Loyalty service unavailable for reservation cancellation, adding to retry queue");
                
                _retryQueue.Enqueue(new RetryItem
                {
                    OperationType = "UpdateLoyaltyAfterCancellation",
                    Username = username,
                    Data = new { Increment = false },
                    Action = async () =>
                    {
                        try
                        {
                            await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, false);
                            return true;
                        }
                        catch (Exception e)
                        {
                            return false;
                        }
                    }
                });
            }

            _logger.LogInformation("Reservation {ReservationUid} cancelled successfully", reservationUid);
            return ServiceResponse<bool>.Success(true);
        }
        catch (ReservationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Reservation {ReservationUid} not found for cancellation", reservationUid);
            return ServiceResponse<bool>.ErrorResponse("Бронирование не найдено", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationUid} for user {Username}", 
                reservationUid, username);
            return ServiceResponse<bool>.ErrorResponse("Ошибка при отмене бронирования", 500);
        }
    }

    public async Task<ServiceResponse<LoyaltyInfoDto>> GetLoyaltyInfoAsync(string username)
    {
        try
        {
            var loyaltyInfo = await _loyaltyClient.GetLoyaltyAsync(username);
            if (loyaltyInfo == null)
            {
                return ServiceResponse<LoyaltyInfoDto>.ErrorResponse(
                    "Информация о программе лояльности не найдена", 404);
            }

            return ServiceResponse<LoyaltyInfoDto>.Success(loyaltyInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty info for {Username}", username);
            return ServiceResponse<LoyaltyInfoDto>.ErrorResponse(
                "Ошибка при получении информации о лояльности", 500);
        }
    }

    // Вспомогательные методы
    private async Task<ReservationDtoWithHotelAndPayment?> GetReservationDetailsAsync(ReservationDto reservation)
    {
        try
        {
            // Получаем информацию об отеле с fallback
            HotelDto? hotel = null;
            try
            {
                hotel = await _reservationClient.GetHotelByIdAsync(reservation.HotelUid);
            }
            catch (HotelNotFoundException ex)
            {
                _logger.LogWarning(ex, "Hotel {HotelUid} not found for reservation {ReservationUid}", 
                    reservation.HotelUid, reservation.ReservationUid);
                hotel = CreateFallbackHotel(reservation.HotelUid);
            }

            // Получаем информацию о платеже с fallback
            PaymentInfoDto? payment = null;
            try
            {
                payment = await _paymentClient.GetPaymentAsync(reservation.PaymentUid);
            }
            catch (PaymentNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment {PaymentUid} not found for reservation {ReservationUid}", 
                    reservation.PaymentUid, reservation.ReservationUid);
                payment = CreateFallbackPayment();
            }

            var fullAddress = hotel != null ? 
                $"{hotel.Country}, {hotel.City}, {hotel.Address}" : 
                "Адрес недоступен";

            var hotelDtoWithFullAddress = new HotelDtoWithFullAddress(
                reservation.HotelUid,
                hotel?.Name ?? "Неизвестный отель",
                fullAddress,
                hotel?.Stars ?? 0
            );

            return new ReservationDtoWithHotelAndPayment(
                reservation.ReservationUid,
                hotelDtoWithFullAddress,
                DateOnly.FromDateTime(reservation.StartDate),
                DateOnly.FromDateTime(reservation.EndDate),
                reservation.Status,
                payment
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting details for reservation {ReservationUid}", reservation.ReservationUid);
            return null;
        }
    }

    private async Task<LoyaltyInfoDto> GetLoyaltyWithFallback(string username)
    {
        try
        {
            var loyaltyInfo = await _loyaltyClient.GetLoyaltyAsync(username);
            return loyaltyInfo ?? new LoyaltyInfoDto("UNKNOWN", 0,0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Loyalty service unavailable for user {Username}, using fallback", username);
            return new LoyaltyInfoDto("UNKNOWN", 0,0);
        }
    }

    private (int TotalPrice, int CountDays) CalculateReservationPrice(
        CreateReservationRequest request, HotelDto hotel, LoyaltyInfoDto loyalty)
    {
        var difference = request.EndDate - request.StartDate;
        var countDays = difference.Days;
        var price = countDays * hotel.Price * (100 - loyalty.Discount) / 100;

        _logger.LogInformation("Price calculation: {Days} days, {BasePrice} base, {Discount}% discount, {FinalPrice} final",
            countDays, hotel.Price, loyalty.Discount, price);

        return (price, countDays);
    }

    private HotelDto CreateFallbackHotel(Guid hotelUid)
    {
        return new HotelDto(hotelUid, "Неизвестный отель", "Неизвестно", "Неизвестно", "Неизвестно", 0, 0);
    }

    private PaymentInfoDto CreateFallbackPayment()
    {
        return new PaymentInfoDto("UNKNOWN", 0);
    }
}

// Классы для работы с retry механизмом
public interface IRetryQueue
{
    void Enqueue(RetryItem item);
}

public class RetryItem
{
    public string OperationType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public Func<Task<bool>> Action { get; set; } = null!;
}

// Класс для унифицированного ответа сервиса
public class ServiceResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Response { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static ServiceResponse<T> Success(T response)
    {
        return new ServiceResponse<T>
        {
            IsSuccess = true,
            Response = response,
            StatusCode = 200
        };
    }

    public static ServiceResponse<T> ErrorResponse(string errorMessage, int statusCode)
    {
        return new ServiceResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}