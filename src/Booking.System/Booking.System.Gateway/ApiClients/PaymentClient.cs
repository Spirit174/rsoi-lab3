using System.Net;
using System.Text.Json.Serialization;
using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Booking.System.Gateway.ApiClients;

public class PaymentClient: IPaymentClient
{
    private readonly ClientsConfiguration _clientsConfiguration;
    private readonly RestClient _client;
    private readonly ILogger<LoyaltyClient> _logger;

    public PaymentClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<LoyaltyClient> logger)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _client = new RestClient("http://payment_service:8060/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
    }
    
    public async Task<PaymentInfoDto> GetPaymentAsync(Guid paymentId)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/payment/{paymentId}";

            request = new RestRequest(requestUrl, Method.Get);

            _logger.LogDebug("Payment API call {Method} {RequestUrl}. To get payment by id {PaymentId}",
                request.Method, requestUrl, paymentId);

            var response = await _client.GetAsync(request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new PaymentNotFoundException("No payment was found");

            var paymentInfoDto = JsonConvert.DeserializeObject<PaymentInfoDto>(response.Content!);

            _logger.LogInformation("Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was got",
                request.Method, requestUrl, paymentId);

            return paymentInfoDto!;
        }
        catch (PaymentNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }

    public async Task UpdatePaymentAsync(Guid paymentId)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/payment/{paymentId}";

            request = new RestRequest(requestUrl, Method.Put);

            _logger.LogDebug("Payment API call {Method} {RequestUrl}. To update payment by id {PaymentId}",
                request.Method, requestUrl, paymentId);

            var response = await _client.PutAsync(request);
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new PaymentNotFoundException("No payment was found");

            _logger.LogInformation(
                "Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was updated",
                request.Method, requestUrl, paymentId);
        }
        catch (PaymentNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    public async Task<Guid> CreatePaymentAsync(int price)
    {
        RestRequest? request = null;
        try
        {
            var requestUrl = $"api/v1/payment/{price}";

            request = new RestRequest(requestUrl, Method.Post);

            _logger.LogDebug("Payment API call {Method} {RequestUrl}. To create payment with price {Price}",
                request.Method, requestUrl, price);

            var response = await _client.PostAsync(request);
            
            var paymentIdDto = JsonConvert.DeserializeObject<PaymentIdDto>(response.Content!);

            _logger.LogInformation(
                "Payment API call {Method} {RequestUrl} successfully. Payment {Price} was updated",
                request.Method, requestUrl, price);

            return paymentIdDto!.PaymentId;
        }
        catch (PaymentNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Payment API returned error status code.", e);
        }
    }
    
    private class PaymentIdDto
    {
        [JsonPropertyName("paymentId")]
        public Guid PaymentId { get; set; }

        public PaymentIdDto(Guid paymentId)
        {
            PaymentId = paymentId;
        }
    }
}