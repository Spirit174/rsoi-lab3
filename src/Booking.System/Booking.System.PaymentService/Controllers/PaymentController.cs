using Booking.System.PaymentService.Core.Interfaces;
using Booking.System.PaymentService.DTO.Converters;
using Booking.System.PaymentService.DTO.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking.System.PaymentService.Controllers;

[ApiController]
[Route("/api/v1")]
public class PaymentController: ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    
    public PaymentController(IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }
    
    /// <summary>
    /// Отменить платеж.
    /// </summary>
    [HttpPut("payment/{paymentId}")]
    public async Task<ActionResult> UpdatePayment([FromRoute] Guid paymentId)
    {
        _logger.LogInformation($"UpdatePayment: {paymentId}");
        try
        {
            if (!await _paymentService.CancelPayment(paymentId))
            {
                return BadRequest(new ErrorResponse("Нeт такого платежа."));
            }

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in payment service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Создать платеж.
    /// </summary>
    [HttpPost("payment/{price}")]
    public async Task<ActionResult<PaymentIdDto>> CreatePayment([FromRoute] int price)
    {
        _logger.LogInformation($"CreatePayment: {price}");
        try
        {
            var paymentId = await _paymentService.CreatePayment(price);

            return Ok(new PaymentIdDto(paymentId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in payment service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    /// <summary>
    /// Получить платеж по идентификатору.
    /// </summary>
    [HttpGet("payment/{paymentId}")]
    public async Task<ActionResult<PaymentInfoDto>> GetPayment([FromRoute] Guid paymentId)
    {
        _logger.LogInformation($"GetPayment: {paymentId}");
        try
        {
            var payment = await _paymentService.GetPayment(paymentId);
            
            if (payment is null)
                return BadRequest(new ErrorResponse("Нeт такого платежа."));
            
            return Ok(PaymentInfoDtoConverter.Convert(payment));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in payment service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }
}