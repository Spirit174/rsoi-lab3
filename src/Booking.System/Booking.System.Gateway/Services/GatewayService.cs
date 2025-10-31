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
        var response = await _reservationClient.GetHotelsPageAsync(page, size);
        
        if (!response.IsSuccess)
        {
            _logger.LogError("Error getting hotels page {Page} size {Size}: {Error}", 
                page, size, response.GetErrorMessage());
            return response;
        }
        
        return response;
    }

    public async Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync(string username)
    {
        // 1. Получаем бронирования
        var reservationsResponse = await _reservationClient.GetReservationByUsername(username);
        if (!reservationsResponse.IsSuccess)
        {
            _logger.LogWarning("Error getting reservations for user {Username}: {Error}", 
                username, reservationsResponse.GetErrorMessage());
            // Для метода /me продолжаем с пустыми бронированиями
        }

        var reservations = reservationsResponse.IsSuccess ? reservationsResponse.Response : new List<ReservationDto>();

        var reservationsWithDetails = new List<ReservationDtoWithHotelAndPayment>();

        foreach (var reservation in reservations)
        {
            var reservationDetail = await GetReservationDetailsAsync(reservation);
            if (reservationDetail != null)
            {
                reservationsWithDetails.Add(reservationDetail);
            }
        }

        // 2. Получаем информацию о лояльности
        var loyaltyResponse = await _loyaltyClient.GetLoyaltyAsync(username);
        var loyaltyInfo = loyaltyResponse.IsSuccess ? 
            loyaltyResponse.Response : 
            new LoyaltyInfoDto("BRONZE", 0, 0);

        var userInfo = new UserInfoDto(reservationsWithDetails, loyaltyInfo);
        return ServiceResponse<UserInfoDto>.Success(userInfo);
    }

    public async Task<ServiceResponse<List<ReservationDtoWithHotelAndPayment>>> GetUserReservationsAsync(string username)
    {
        var reservationsResponse = await _reservationClient.GetReservationByUsername(username);
        
        if (!reservationsResponse.IsSuccess)
        {
            _logger.LogError("Error getting reservations for {Username}: {Error}", 
                username, reservationsResponse.GetErrorMessage());
            return ServiceResponse<List<ReservationDtoWithHotelAndPayment>>.ErrorResponse(
                reservationsResponse.GetErrorMessage(), reservationsResponse.StatusCode);
        }

        var reservations = reservationsResponse.Response ?? new List<ReservationDto>();
        var result = new List<ReservationDtoWithHotelAndPayment>();

        foreach (var reservation in reservations)
        {
            var reservationDetail = await GetReservationDetailsAsync(reservation);
            if (reservationDetail != null)
            {
                result.Add(reservationDetail);
            }
        }

        return ServiceResponse<List<ReservationDtoWithHotelAndPayment>>.Success(result);
    }

    public async Task<ServiceResponse<ReservationDtoWithHotelAndPayment?>> GetReservationAsync(string username, Guid reservationUid)
    {
        var reservationResponse = await _reservationClient.GetReservationById(reservationUid);
        
        if (!reservationResponse.IsSuccess)
        {
            _logger.LogWarning("Reservation {ReservationUid} not found for user {Username}: {Error}", 
                reservationUid, username, reservationResponse.GetErrorMessage());
            return ServiceResponse<ReservationDtoWithHotelAndPayment?>.ErrorResponse(
                reservationResponse.GetErrorMessage(), reservationResponse.StatusCode);
        }

        var reservation = reservationResponse.Response;
        if (reservation == null)
        {
            return ServiceResponse<ReservationDtoWithHotelAndPayment?>.ErrorResponse(
                "Бронирование не найдено", 404);
        }

        var reservationDetail = await GetReservationDetailsAsync(reservation);
        return ServiceResponse<ReservationDtoWithHotelAndPayment?>.Success(reservationDetail);
    }

    public async Task<ServiceResponse<CreateReservationResponse?>> CreateReservationAsync(
        string username, CreateReservationRequest request)
    {
        _logger.LogInformation("Starting reservation creation for user: {Username}, hotel: {HotelUid}", 
            username, request.HotelUid);

        // 1. Получаем информацию об отеле
        var hotelResponse = await _reservationClient.GetHotelByIdAsync(request.HotelUid);
        if (!hotelResponse.IsSuccess)
        {
            _logger.LogWarning("Hotel {HotelUid} not found for reservation creation: {Error}", 
                request.HotelUid, hotelResponse.GetErrorMessage());
            return ServiceResponse<CreateReservationResponse?>.ErrorResponse(
                "Отель не найден", 404);
        }
        var hotel = hotelResponse.Response!;

        // 2. Получаем информацию о лояльности
        var loyaltyResponse = await _loyaltyClient.GetLoyaltyAsync(username);
        var loyaltyInfo = loyaltyResponse.IsSuccess ? 
            loyaltyResponse.Response! : 
            new LoyaltyInfoDto("BRONZE", 0, 0);

        // 3. Рассчитываем стоимость
        var (totalPrice, countDays) = CalculateReservationPrice(request, hotel, loyaltyInfo);

        // 4. Создаем платеж
        var paymentUidResponse = await _paymentClient.CreatePaymentAsync(totalPrice);
        if (!paymentUidResponse.IsSuccess)
        {
            _logger.LogError("Error creating payment for reservation: {Error}", paymentUidResponse.GetErrorMessage());
            return ServiceResponse<CreateReservationResponse?>.ErrorResponse(
                "Ошибка при создании платежа", 500);
        }
        var paymentUid = paymentUidResponse.Response;

        var paymentResponse = await _paymentClient.GetPaymentAsync(paymentUid);
        var payment = paymentResponse.IsSuccess ? paymentResponse.Response! : new PaymentInfoDto("PAID", totalPrice);

        // 5. Создаем бронирование
        var reservationUid = Guid.NewGuid();
        var createReservationResponse = await _reservationClient.CreateReservation(new CreateReservationDto(
            reservationUid, username, paymentUid, request.HotelUid, request.StartDate, request.EndDate));

        if (!createReservationResponse.IsSuccess)
        {
            _logger.LogError("Error creating reservation: {Error}", createReservationResponse.GetErrorMessage());
            return ServiceResponse<CreateReservationResponse?>.ErrorResponse(
                "Ошибка при создании бронирования", 500);
        }

        // 6. Обновляем счетчик бронирований (не критичный)
        var loyaltyUpdateResponse = await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, true);
        if (!loyaltyUpdateResponse.IsSuccess)
        {
            _logger.LogWarning("Loyalty service unavailable for reservation creation, adding to retry queue");
            
            _retryQueue.Enqueue(new RetryItem
            {
                OperationType = "UpdateLoyaltyAfterReservation",
                Username = username,
                Data = new { Increment = true },
                Action = async () =>
                {
                    var retryResponse = await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, true);
                    return retryResponse.IsSuccess;
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

    public async Task<ServiceResponse<bool>> CancelReservationAsync(string username, Guid reservationUid)
    {
        // 1. Получаем бронирование
        var reservationResponse = await _reservationClient.GetReservationById(reservationUid);
        if (!reservationResponse.IsSuccess)
        {
            _logger.LogWarning("Reservation {ReservationUid} not found for cancellation: {Error}", 
                reservationUid, reservationResponse.GetErrorMessage());
            return ServiceResponse<bool>.ErrorResponse("Бронирование не найдено", 404);
        }
        var reservation = reservationResponse.Response!;

        // 2. Отменяем бронирование
        var cancelResponse = await _reservationClient.CancelReservation(reservationUid);
        if (!cancelResponse.IsSuccess)
        {
            _logger.LogError("Error cancelling reservation {ReservationUid}: {Error}", 
                reservationUid, cancelResponse.GetErrorMessage());
            return ServiceResponse<bool>.ErrorResponse("Ошибка при отмене бронирования", 500);
        }

        // 3. Обновляем платеж
        var paymentUpdateResponse = await _paymentClient.UpdatePaymentAsync(reservation.PaymentUid);
        if (!paymentUpdateResponse.IsSuccess)
        {
            _logger.LogWarning("Error updating payment for reservation {ReservationUid}: {Error}", 
                reservationUid, paymentUpdateResponse.GetErrorMessage());
            // Продолжаем, так как это не критично для отмены
        }

        // 4. Обновляем счетчик лояльности (не критичный)
        var loyaltyUpdateResponse = await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, false);
        if (!loyaltyUpdateResponse.IsSuccess)
        {
            _logger.LogWarning("Loyalty service unavailable for reservation cancellation, adding to retry queue");
            
            _retryQueue.Enqueue(new RetryItem
            {
                OperationType = "UpdateLoyaltyAfterCancellation",
                Username = username,
                Data = new { Increment = false },
                Action = async () =>
                {
                    var retryResponse = await _loyaltyClient.UpdateLoyaltyReservationCountAsync(username, false);
                    return retryResponse.IsSuccess;
                }
            });
        }

        _logger.LogInformation("Reservation {ReservationUid} cancelled successfully", reservationUid);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<ServiceResponse<LoyaltyInfoDto>> GetLoyaltyInfoAsync(string username)
    {
        var response = await _loyaltyClient.GetLoyaltyAsync(username);
        
        if (!response.IsSuccess)
        {
            _logger.LogError("Error getting loyalty info for {Username}: {Error}", 
                username, response.GetErrorMessage());
        }
        
        return response;
    }

    // Вспомогательные методы
    private async Task<ReservationDtoWithHotelAndPayment?> GetReservationDetailsAsync(ReservationDto reservation)
    {
        // Получаем информацию об отеле
        HotelDto? hotel = null;
        var hotelResponse = await _reservationClient.GetHotelByIdAsync(reservation.HotelUid);
        if (hotelResponse.IsSuccess)
        {
            hotel = hotelResponse.Response;
        }
        else
        {
            _logger.LogWarning("Hotel {HotelUid} not found for reservation {ReservationUid}: {Error}", 
                reservation.HotelUid, reservation.ReservationUid, hotelResponse.GetErrorMessage());
            hotel = CreateFallbackHotel(reservation.HotelUid);
        }

        // Получаем информацию о платеже
        PaymentInfoDto? payment = null;
        var paymentResponse = await _paymentClient.GetPaymentAsync(reservation.PaymentUid);
        if (paymentResponse.IsSuccess)
        {
            payment = paymentResponse.Response;
        }
        else
        {
            _logger.LogWarning("Payment {PaymentUid} not found for reservation {ReservationUid}: {Error}", 
                reservation.PaymentUid, reservation.ReservationUid, paymentResponse.GetErrorMessage());
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