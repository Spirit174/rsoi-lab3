using System.Net;
using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Booking.System.Gateway.ApiClients;

public class ReservationClient: IReservationClient
{
    private readonly ClientsConfiguration _clientsConfiguration;
    private readonly RestClient _client;
    private readonly ILogger<ReservationClient> _logger;
    private readonly CircuitBreaker.CircuitBreaker _circuitBreaker;

    public ReservationClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<ReservationClient> logger, CircuitBreaker.CircuitBreaker circuitBreaker)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _client = new RestClient("http://reservation_service:8070/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
        
        _circuitBreaker.RegisterHealthCheck("ReservationService", HealthCheckAsync);
    }

    private async Task<bool> HealthCheckAsync()
    {
        try
        {
            var request = new RestRequest("manage/health", Method.Get);
            var response = await _client.ExecuteAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<HotelPagesDto> GetHotelsPageAsync(int page, int size)
    {
        // Reservation Service критичен для GET /api/v1/hotels - бросаем исключение
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/hotels?page={page}&size={size}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get hotels page:{Page} with size:{Size}",
                    request.Method, requestUrl, page, size);

                var response = await _client.GetAsync(request);
                var hotelsPages = JsonConvert.DeserializeObject<HotelPagesDto>(response.Content!);

                _logger.LogInformation("Reservation API call {Method} {RequestUrl} successfully. Got hotels page:{Page} with size:{Size}",
                    request.Method, requestUrl, page, size);

                return hotelsPages!;
            },
            () => throw new Exception("Reservation Service is unavailable for hotels operation"));
    }
    
    public async Task<HotelDto> GetHotelByIdAsync(Guid hotelId)
    {
        // Получение отеля по ID критично
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/hotels/{hotelId}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get hotel by Id {HotelId}",
                    request.Method, requestUrl, hotelId);

                var response = await _client.GetAsync(request);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new HotelNotFoundException("No hotel was found");

                var hotel = JsonConvert.DeserializeObject<HotelDto>(response.Content!);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To got hotel by Id {HotelId}",
                    request.Method, requestUrl, hotelId);

                return hotel!;
            },
            () => throw new Exception("Reservation Service is unavailable"));
    }
    
    public async Task CancelReservation(Guid reservationId)
    {
        // Отмена бронирования критична
        await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations/{reservationId}";
                var request = new RestRequest(requestUrl, Method.Post);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To cancel reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                var response = await _client.PostAsync(request);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new ReservationNotFoundException("No reservation was found");

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To cancel reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                return true;
            },
            () => throw new Exception("Reservation Service is unavailable for cancel operation"));
    }
    
    public async Task<ReservationDto> GetReservationById(Guid reservationId)
    {
        // Reservation Service критичен для получения бронирования
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations/{reservationId}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                var response = await _client.GetAsync(request);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new ReservationNotFoundException("No reservation was found");
                
                var reservation = JsonConvert.DeserializeObject<ReservationDto>(response.Content!);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To got reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                return reservation!;
            },
            () => throw new Exception("Reservation Service is unavailable"));
    }
    
    public async Task CreateReservation(CreateReservationDto createReservationDto)
    {
        // Создание бронирования критично
        await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations";
                var request = new RestRequest(requestUrl, Method.Post)
                    .AddJsonBody(createReservationDto);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To create reservation with PaymentUid {PaymentUid}",
                    request.Method, requestUrl, createReservationDto.PaymentUid);

                await _client.PostAsync(request);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. Created reservation with PaymentUid {PaymentUid}",
                    request.Method, requestUrl, createReservationDto.PaymentUid);

                return true;
            },
            () => throw new Exception("Reservation Service is unavailable for reservation creation"));
    }
    
    public async Task<List<ReservationDto>> GetReservationByUsername(string userName)
    {
        // Получение списка бронирований критично
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations/user/{userName}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get reservation by username {UserName}",
                    request.Method, requestUrl, userName);

                var response = await _client.GetAsync(request);
                
                var reservations = JsonConvert.DeserializeObject<List<ReservationDto>>(response.Content!);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To got reservation by username {UserName}",
                    request.Method, requestUrl, userName);

                return reservations!;
            },
            () => throw new Exception("Reservation Service is unavailable"));
    }
}