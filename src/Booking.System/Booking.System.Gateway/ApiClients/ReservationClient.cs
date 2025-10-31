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

    public async Task<ServiceResponse<HotelPagesDto>> GetHotelsPageAsync(int page, int size)
{
    return await _circuitBreaker.ExecuteAsync(
        "ReservationService",
        async () =>
        {
            var requestUrl = $"api/v1/hotels?page={page}&size={size}";
            var request = new RestRequest(requestUrl, Method.Get);

            _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get hotels page:{Page} with size:{Size}",
                request.Method, requestUrl, page, size);

            var response = await _client.GetAsync(request);
            
            if (!response.IsSuccessful)
            {
                return ServiceResponse<HotelPagesDto>.ErrorResponse(
                    $"Reservation API returned error: {response.StatusCode}", 
                    (int)response.StatusCode);
            }

            var hotelsPages = JsonConvert.DeserializeObject<HotelPagesDto>(response.Content!);

            _logger.LogInformation("Reservation API call {Method} {RequestUrl} successfully. Got hotels page:{Page} with size:{Size}",
                request.Method, requestUrl, page, size);

            return ServiceResponse<HotelPagesDto>.Success(hotelsPages!);
        },
        ServiceResponse<HotelPagesDto>.ServiceUnavailable("Reservation Service"));
}

public async Task<ServiceResponse<HotelDto>> GetHotelByIdAsync(Guid hotelId)
{
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
            {
                return ServiceResponse<HotelDto>.ErrorResponse(
                    "No hotel was found", 
                    (int)HttpStatusCode.NotFound);
            }

            if (!response.IsSuccessful)
            {
                return ServiceResponse<HotelDto>.ErrorResponse(
                    $"Reservation API returned error: {response.StatusCode}", 
                    (int)response.StatusCode);
            }

            var hotel = JsonConvert.DeserializeObject<HotelDto>(response.Content!);

            _logger.LogInformation(
                "Reservation API call {Method} {RequestUrl} successfully. To got hotel by Id {HotelId}",
                request.Method, requestUrl, hotelId);

            return ServiceResponse<HotelDto>.Success(hotel!);
        },
        ServiceResponse<HotelDto>.ServiceUnavailable("Reservation Service"));
}
    
    public async Task<ServiceResponse<bool>> CancelReservation(Guid reservationId)
    {
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations/{reservationId}";
                var request = new RestRequest(requestUrl, Method.Post);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To cancel reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                var response = await _client.PostAsync(request);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "No reservation was found", 
                        (int)HttpStatusCode.NotFound);
                }

                if (!response.IsSuccessful)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        $"Reservation API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To cancel reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                return ServiceResponse<bool>.Success(true);
            },
            ServiceResponse<bool>.ServiceUnavailable("Reservation Service"));
    }
    
    public async Task<ServiceResponse<ReservationDto>> GetReservationById(Guid reservationId)
    {
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
                {
                    return ServiceResponse<ReservationDto>.ErrorResponse(
                        "No reservation was found", 
                        (int)HttpStatusCode.NotFound);
                }

                if (!response.IsSuccessful)
                {
                    return ServiceResponse<ReservationDto>.ErrorResponse(
                        $"Reservation API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }
            
                var reservation = JsonConvert.DeserializeObject<ReservationDto>(response.Content!);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To got reservation by Id {ReservationId}",
                    request.Method, requestUrl, reservationId);

                return ServiceResponse<ReservationDto>.Success(reservation!);
            },
            ServiceResponse<ReservationDto>.ServiceUnavailable("Reservation Service"));
    }
    
    public async Task<ServiceResponse<bool>> CreateReservation(CreateReservationDto createReservationDto)
    {
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations";
                var request = new RestRequest(requestUrl, Method.Post)
                    .AddJsonBody(createReservationDto);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To create reservation with PaymentUid {PaymentUid}",
                    request.Method, requestUrl, createReservationDto.PaymentUid);

                var response = await _client.PostAsync(request);

                if (!response.IsSuccessful)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        $"Reservation API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. Created reservation with PaymentUid {PaymentUid}",
                    request.Method, requestUrl, createReservationDto.PaymentUid);

                return ServiceResponse<bool>.Success(true);
            },
            ServiceResponse<bool>.ServiceUnavailable("Reservation Service"));
    }
    
    public async Task<ServiceResponse<List<ReservationDto>>> GetReservationByUsername(string userName)
    {
        return await _circuitBreaker.ExecuteAsync(
            "ReservationService",
            async () =>
            {
                var requestUrl = $"api/v1/reservations/user/{userName}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get reservation by username {UserName}",
                    request.Method, requestUrl, userName);

                var response = await _client.GetAsync(request);
            
                if (!response.IsSuccessful)
                {
                    return ServiceResponse<List<ReservationDto>>.ErrorResponse(
                        $"Reservation API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }
            
                var reservations = JsonConvert.DeserializeObject<List<ReservationDto>>(response.Content!);

                _logger.LogInformation(
                    "Reservation API call {Method} {RequestUrl} successfully. To got reservation by username {UserName}",
                    request.Method, requestUrl, userName);

                return ServiceResponse<List<ReservationDto>>.Success(reservations!);
            },
            ServiceResponse<List<ReservationDto>>.ServiceUnavailable("Reservation Service"));
    }
}