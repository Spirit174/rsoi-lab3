using Booking.System.PaymentService.Core.Interfaces;
using Booking.System.PaymentService.Core.Models;
using Booking.System.PaymentService.Core.Models.Enums;

namespace Booking.System.PaymentService.Core.Services;

public class PaymentService: IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IPaymentRepository _paymentRepository;
    
    public PaymentService(ILogger<PaymentService> logger,
        IPaymentRepository paymentRepository)
    {
        _logger = logger;
        _paymentRepository = paymentRepository;
    }
    
    
    public async Task<Guid> CreatePayment(int price)
    {
        _logger.LogDebug("Creating payment with price: {Price}", price);
        
        var paymentId = Guid.NewGuid();
        var payment = new Payment(default, paymentId, PaymentStatus.PAID, price);
        
        await _paymentRepository.CreatePaymentAsync(payment);
        _logger.LogInformation("Created payment with price: {Price} and {PaymentId}", price, paymentId);
        
        return paymentId;
    }
    
    public async Task<bool> CancelPayment(Guid paymentId)
    {
        return await _paymentRepository.UpdatePaymentAsync(paymentId);
    }
    
    public async Task<Payment?> GetPayment(Guid paymentId)
    {
        return await _paymentRepository.GetPaymentByPaymentIdAsync(paymentId);
    }
}