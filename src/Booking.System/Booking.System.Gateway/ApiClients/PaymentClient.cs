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
    
    public async Task<PaymentInfoDto> GetPaymentAsync(Guid paymentId)
    {
        // Payment Service не критичен для операций получения - возвращаем fallback
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
                    throw new PaymentNotFoundException("No payment was found");

                var paymentInfoDto = JsonConvert.DeserializeObject<PaymentInfoDto>(response.Content!);

                _logger.LogInformation("Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was got",
                    request.Method, requestUrl, paymentId);

                return paymentInfoDto!;
            },
            () => 
            {
                _logger.LogWarning("Payment Service unavailable, returning fallback for payment {PaymentId}", paymentId);
                // Возвращаем fallback с пустыми данными о платеже
                return new PaymentInfoDto("UNKNOWN", 0);
            });
    }

    public async Task UpdatePaymentAsync(Guid paymentId)
    {
        // Для операций обновления считаем сервис критичным
        await _circuitBreaker.ExecuteAsync(
            "PaymentService",
            async () =>
            {
                var requestUrl = $"api/v1/payment/{paymentId}";
                var request = new RestRequest(requestUrl, Method.Put);

                _logger.LogDebug("Payment API call {Method} {RequestUrl}. To update payment by id {PaymentId}",
                    request.Method, requestUrl, paymentId);

                var response = await _client.PutAsync(request);
                
                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new PaymentNotFoundException("No payment was found");

                _logger.LogInformation(
                    "Payment API call {Method} {RequestUrl} successfully. Payment {PaymentId} was updated",
                    request.Method, requestUrl, paymentId);

                return true;
            },
            () => 
            {
                _logger.LogError("Payment Service unavailable for critical update operation for payment {PaymentId}", paymentId);
                throw new Exception("Payment Service is unavailable for update operation");
            });
    }
    
    public async Task<Guid> CreatePaymentAsync(int price)
    {
        // Создание платежа критично - бросаем исключение при недоступности
        return await _circuitBreaker.ExecuteAsync(
            "PaymentService",
            async () =>
            {
                var requestUrl = $"api/v1/payment/{price}";
                var request = new RestRequest(requestUrl, Method.Post);

                _logger.LogDebug("Payment API call {Method} {RequestUrl}. To create payment with price {Price}",
                    request.Method, requestUrl, price);

                var response = await _client.PostAsync(request);
                
                var paymentIdDto = JsonConvert.DeserializeObject<PaymentIdDto>(response.Content!);

                _logger.LogInformation(
                    "Payment API call {Method} {RequestUrl} successfully. Payment with price {Price} was created",
                    request.Method, requestUrl, price);

                return paymentIdDto!.PaymentId;
            },
            () => 
            {
                _logger.LogError("Payment Service unavailable for critical payment creation with price {Price}", price);
                throw new Exception("Payment Service is unavailable for payment creation");
            });
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