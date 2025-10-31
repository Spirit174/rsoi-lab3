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

    public ReservationClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<ReservationClient> logger)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _client = new RestClient("http://reservation_service:8070/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
    }

    public async Task<HotelPagesDto> GetHotelsPageAsync(int page, int size)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/hotels?page={page}&size={size}";

            request = new RestRequest(requestUrl, Method.Get);

            _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get hotels page:{Page} with size:{Size}",
                request.Method, requestUrl, page, size);

            var response = await _client.GetAsync(request);

            var hotelsPages = JsonConvert.DeserializeObject<HotelPagesDto>(response.Content!);

            _logger.LogInformation("Reservation API call {Method} {RequestUrl} successfully. Got hotels page:{Page} with size:{Size}",
                request.Method, requestUrl, page, size);

            return hotelsPages!;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task<HotelDto> GetHotelByIdAsync(Guid hotelId)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/hotels/{hotelId}";

            request = new RestRequest(requestUrl, Method.Get);

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
        }
        catch(HotelNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task CancelReservation(Guid reservationId)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/reservations/{reservationId}";

            request = new RestRequest(requestUrl, Method.Post);

            _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To cancel reservation by Id {ReservationId}",
                request.Method, requestUrl, reservationId);

            var response = await _client.PostAsync(request);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ReservationNotFoundException("No reservation was found");

            _logger.LogInformation(
                "Reservation API call {Method} {RequestUrl} successfully. To cancel reservation by Id {ReservationId}",
                request.Method, requestUrl, reservationId);
        }
        catch(ReservationNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task<ReservationDto> GetReservationById(Guid reservationId)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/reservations/{reservationId}";

            request = new RestRequest(requestUrl, Method.Get);

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
        }
        catch(ReservationNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task CreateReservation(CreateReservationDto createReservationDto)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/reservations";

            request = new RestRequest(requestUrl, Method.Post)
                .AddJsonBody(createReservationDto);

            _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To create reservation by Id {ReservationId}",
                request.Method, requestUrl, createReservationDto.PaymentUid);

            await _client.PostAsync(request);

            _logger.LogInformation(
                "Reservation API call {Method} {RequestUrl} successfully. To created reservation by Id {ReservationId}",
                request.Method, requestUrl, createReservationDto.PaymentUid);
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task<List<ReservationDto>> GetReservationByUsername(string userName)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/reservations/user/{userName}";

            request = new RestRequest(requestUrl, Method.Get);

            _logger.LogDebug("Reservation API call {Method} {RequestUrl}. To get reservation by username {UserName}",
                request.Method, requestUrl, userName);

            var response = await _client.GetAsync(request);
            
            var reservations = JsonConvert.DeserializeObject<List<ReservationDto>>(response.Content!);

            _logger.LogInformation(
                "Reservation API call {Method} {RequestUrl} successfully. To got reservation by username {UserName}",
                request.Method, requestUrl, userName);

            return reservations!;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
}