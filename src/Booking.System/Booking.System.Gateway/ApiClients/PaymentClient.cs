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
    private readonly ILogger<PaymentClient> _logger;
    private readonly CircuitBreaker.CircuitBreaker _circuitBreaker;

    public PaymentClient(IOptions<ClientsConfiguration> clientsConfiguration,
        ILogger<PaymentClient> logger, CircuitBreaker.CircuitBreaker circuitBreaker)
    {
        _clientsConfiguration = clientsConfiguration.Value;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _client = new RestClient("http://payment_service:8060/",
            configureRestClient: c => { c.ThrowOnAnyError = true; },
            configureSerialization: s => { s.UseNewtonsoftJson(); });
        
        _circuitBreaker.RegisterHealthCheck("PaymentService", HealthCheckAsync);
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
    
    public async Task<ServiceResponse<PaymentInfoDto>> GetPaymentAsync(Guid paymentId)
    {
        return await _circuitBreaker.ExecuteAsync(
            "PaymentService",
            async () =>
            {
                var requestUrl = $"api/v1/payment/{paymentId}";
                var request = new RestRequest(requestUrl, Method.Get);

                _logger.LogDebug("Payment API call {Method} {RequestUrl}. To get payment by id {PaymentId}",
                    request.Method, requestUrl, paymentId);

                var response = await _client.GetAsync(request);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return ServiceResponse<PaymentInfoDto>.ErrorResponse(
                        "No payment was found", 
                        (int)HttpStatusCode.BadRequest);
                }

                if (!response.IsSuccessful)
                {
                    return ServiceResponse<PaymentInfoDto>.ErrorResponse(
                        $"Payment API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                var paymentInfoDto = JsonConvert.DeserializeObject<PaymentInfoDto>(response.Content!);

                _logger.LogInformation("Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was got",
                    request.Method, requestUrl, paymentId);

                return ServiceResponse<PaymentInfoDto>.Success(paymentInfoDto!);
            },
            ServiceResponse<PaymentInfoDto>.Fallback(new PaymentInfoDto("UNKNOWN", 0)));
    }

    public async Task<ServiceResponse<bool>> UpdatePaymentAsync(Guid paymentId)
    {
        return await _circuitBreaker.ExecuteAsync(
            "PaymentService",
            async () =>
            {
                var requestUrl = $"api/v1/payment/{paymentId}";
                var request = new RestRequest(requestUrl, Method.Put);

                _logger.LogDebug("Payment API call {Method} {RequestUrl}. To update payment by id {PaymentId}",
                    request.Method, requestUrl, paymentId);

                var response = await _client.PutAsync(request);
            
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "No payment was found", 
                        (int)HttpStatusCode.BadRequest);
                }

                if (!response.IsSuccessful)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        $"Payment API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                _logger.LogInformation(
                    "Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was updated",
                    request.Method, requestUrl, paymentId);

                return ServiceResponse<bool>.Success(true);
            },
            ServiceResponse<bool>.ServiceUnavailable("Payment Service")); // Для критичных операций
    }
    public async Task<ServiceResponse<Guid>> CreatePaymentAsync(int price)
    {
        return await _circuitBreaker.ExecuteAsync(
            "PaymentService",
            async () =>
            {
                var requestUrl = $"api/v1/payment/{price}";
                var request = new RestRequest(requestUrl, Method.Post);

                _logger.LogDebug("Payment API call {Method} {RequestUrl}. To create payment with price {Price}",
                    request.Method, requestUrl, price);

                var response = await _client.PostAsync(request);
            
                // Обработка ошибок HTTP
                if (!response.IsSuccessful)
                {
                    return ServiceResponse<Guid>.ErrorResponse(
                        $"Payment API returned error: {response.StatusCode}", 
                        (int)response.StatusCode);
                }

                var paymentIdDto = JsonConvert.DeserializeObject<PaymentIdDto>(response.Content!);

                _logger.LogInformation(
                    "Payment API call {Method} {RequestUrl} successfully. Payment with price {Price} was created",
                    request.Method, requestUrl, price);

                return ServiceResponse<Guid>.Success(paymentIdDto!.PaymentId);
            },
            ServiceResponse<Guid>.ServiceUnavailable("Payment Service"));
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