using Booking.System.Gateway.DTO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Booking.System.Gateway.ApiClients;

public class LoyaltyClient : ILoyaltyClient
{
    private readonly ClientsConfiguration _clientsConfiguration;
    private readonly RestClient _client;
    private readonly ILogger<LoyaltyClient> _logger;
    private readonly CircuitBreaker.CircuitBreaker _circuitBreaker;

    public LoyaltyClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<LoyaltyClient> logger, CircuitBreaker.CircuitBreaker circuitBreaker)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _client = new RestClient("http://loyalty_service:8050/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
        
        _circuitBreaker.RegisterHealthCheck("LoyaltyService", HealthCheckAsync);
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

    public async Task<ServiceResponse<LoyaltyInfoDto>> GetLoyaltyAsync(string userName)
    {
        return await _circuitBreaker.ExecuteAsync(
            "LoyaltyService",
            async () =>
            {
                var requestUrl = $"api/v1/loyalty/{userName}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Loyalty API call {Method} {RequestUrl}. To get loyalty for username {UserName}",
                    request.Method, requestUrl, userName);

                var response = await _client.GetAsync(request);

                // Обработка ошибок HTTP
                if (!response.IsSuccessful)
                {
                    return ServiceResponse<LoyaltyInfoDto>.ErrorResponse(
                        $"Loyalty API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                var loyaltyInfoDto = JsonConvert.DeserializeObject<LoyaltyInfoDto>(response.Content!);

                _logger.LogInformation("Loyalty API call {Method} {RequestUrl} successfully. Loyalty {UserName} was got",
                    request.Method, requestUrl, userName);

                return ServiceResponse<LoyaltyInfoDto>.Success(loyaltyInfoDto!);
            },
            ServiceResponse<LoyaltyInfoDto>.ServiceUnavailable("Loyalty"));
    }
    
    public async Task<ServiceResponse<bool>> UpdateLoyaltyReservationCountAsync(string userName, bool isIncrease)
    {
        return await _circuitBreaker.ExecuteAsync(
            "LoyaltyService",
            async () =>
            {
                var requestUrl = $"api/v1/loyalty/{userName}";
                var requestBody = new IncreaseBool
                {
                    IsIncrease = isIncrease
                };

                var request = new RestRequest(requestUrl, Method.Post)
                    .AddJsonBody(requestBody);

                _logger.LogDebug("Loyalty API call {Method} {RequestUrl}. To update loyalty for username {UserName}",
                    request.Method, requestUrl, userName);

                var response = await _client.PostAsync(request);

                // Обработка ошибок HTTP
                if (!response.IsSuccessful)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        $"Loyalty API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                _logger.LogInformation(
                    "Loyalty API call {Method} {RequestUrl} successfully. Loyalty {UserName} was updated",
                    request.Method, requestUrl, userName);

                return ServiceResponse<bool>.Success(true);
            },
            ServiceResponse<bool>.ServiceUnavailable("Loyalty"));
    }

    private class IncreaseBool
    {
        [JsonProperty("isIncrease")]
        public bool IsIncrease { get; set; }
    }
}