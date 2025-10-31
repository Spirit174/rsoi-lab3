using Booking.System.PaymentService.Core.Interfaces;
using Booking.System.PaymentService.Core.Models;
using Booking.System.PaymentService.DataBase.Context;
using Booking.System.PaymentService.DataBase.Converters;
using Booking.System.PaymentService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.PaymentService.DataBase.Repositories;

public class PaymentRepository: IPaymentRepository
{
    private readonly PaymentContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(PaymentContext context,
        ILogger<PaymentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }    
    
    public async Task CreatePaymentAsync(Payment payment)
    {
        _logger.LogDebug("Creating payment with id: {PaymentId}", payment.PaymentUid);
        
        await _context.Payments.AddAsync(PaymentConverter.Convert(payment));
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created loyalty with id: {PaymentId}", payment.PaymentUid);
    }

    public async Task<bool> UpdatePaymentAsync(Guid paymentId)
    {
        _logger.LogDebug("Cancel payment with id: {PaymentId}", paymentId);
        
        var dbPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentUid == paymentId);
        if (dbPayment is null)
        {
            _logger.LogInformation("Cancel payment with id: {PaymentId} not found", paymentId);
            return false;
        }

        dbPayment.PaymentStatus = DbPaymentStatus.CANCELED;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Cancel payment  with id: {PaymentId} canceled", paymentId);

        return true;
    }
    
    public async Task<Payment?> GetPaymentByPaymentIdAsync(Guid paymentId)
    {
        _logger.LogDebug("Getting payment with id: {PaymentId}", paymentId);
        
        var dbPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentUid == paymentId);
        return PaymentConverter.Convert(dbPayment);
    }
}