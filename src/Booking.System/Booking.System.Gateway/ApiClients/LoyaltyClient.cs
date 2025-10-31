using Booking.System.Gateway.DTO;
using Booking.System.LoyaltyService.DTO.Models;
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

    public LoyaltyClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<LoyaltyClient> logger)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _client = new RestClient("http://loyalty_service:8050/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
    }

    public async Task<LoyaltyInfoDto> GetLoyaltyAsync(string userName)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/loyalty/{userName}";

            request = new RestRequest(requestUrl, Method.Get);

            _logger.LogDebug("Loyalty API call {Method} {RequestUrl}. To get loyalty for username {UserName}",
                request.Method, requestUrl, userName);

            var response = await _client.GetAsync(request);

            var loyaltyInfoDto = JsonConvert.DeserializeObject<LoyaltyInfoDto>(response.Content!);

            _logger.LogInformation("Loyalty API call {Method} {RequestUrl} successfully. Loyalty {UserName} was got",
                request.Method, requestUrl, userName);

            return loyaltyInfoDto!;
        }
        catch (Exception e)
        {
            throw new Exception("Loyalty API returned error status code.", e);
        }
    }

    public async Task UpdateLoyaltyReservationCountAsync(string userName, bool isIncrease)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/loyalty/{userName}";

            var requestBody = new IncreaseBool
            {
                IsIncrease = isIncrease
            };

            request = new RestRequest(requestUrl, Method.Post)
                .AddJsonBody(requestBody);

            _logger.LogDebug("Loyalty API call {Method} {RequestUrl}. To update loyalty for username {UserName}",
                request.Method, requestUrl, userName);

            await _client.PostAsync(request);

            _logger.LogInformation(
                "Loyalty API call {Method} {RequestUrl} successfully. Loyalty {UserName} was updated",
                request.Method, requestUrl, userName);
        }
        catch (Exception e)
        {
            throw new Exception("Loyalty API returned error status code.", e);
        }
    }


    private class IncreaseBool
    {
        [JsonProperty("isIncrease")]
        public bool IsIncrease { get; set; }
    }
}