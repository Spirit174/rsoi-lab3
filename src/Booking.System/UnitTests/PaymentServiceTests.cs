using Booking.System.PaymentService.Controllers;
using Booking.System.PaymentService.Core.Interfaces;
using Booking.System.PaymentService.Core.Models;
using Booking.System.PaymentService.DTO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ErrorResponse = Booking.System.PaymentService.DTO.Models.ErrorResponse;

namespace UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<ILogger<PaymentController>> _loggerMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly PaymentController _paymentController;

    public PaymentServiceTests()
    {
        _loggerMock = new Mock<ILogger<PaymentController>>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _paymentController = new PaymentController(_paymentServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePayment_ValidPrice_ReturnsPaymentId()
    {
        // Arrange
        var price = 1000;
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.CreatePayment(price))
            .ReturnsAsync(paymentId);

        // Act
        var result = await _paymentController.CreatePayment(price);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaymentIdDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var paymentIdDto = Assert.IsType<PaymentIdDto>(okResult.Value);
        
        Assert.Equal(paymentId, paymentIdDto.PaymentId);
        _paymentServiceMock.Verify(x => x.CreatePayment(price), Times.Once);
    }

    [Fact]
    public async Task CreatePayment_ServiceThrows_Returns500()
    {
        // Arrange
        var price = 1000;
        
        _paymentServiceMock
            .Setup(x => x.CreatePayment(price))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentController.CreatePayment(price);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaymentIdDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public async Task GetPayment_NonExistingPayment_ReturnsBadRequest()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.GetPayment(paymentId))
            .ReturnsAsync((Payment)null);

        // Act
        var result = await _paymentController.GetPayment(paymentId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaymentInfoDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Нeт такого платежа.", error.Message);
    }

    [Fact]
    public async Task GetPayment_ServiceThrows_Returns500()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.GetPayment(paymentId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentController.GetPayment(paymentId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaymentInfoDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public async Task UpdatePayment_SuccessfulCancel_ReturnsOk()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.CancelPayment(paymentId))
            .ReturnsAsync(false);

        // Act
        var result = await _paymentController.UpdatePayment(paymentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _paymentServiceMock.Verify(x => x.CancelPayment(paymentId), Times.Once);
    }

    [Fact]
    public async Task UpdatePayment_PaymentNotFound_ReturnsBadRequest()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.CancelPayment(paymentId))
            .ReturnsAsync(true);

        // Act
        var result = await _paymentController.UpdatePayment(paymentId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Нeт такого платежа.", error.Message);
        _paymentServiceMock.Verify(x => x.CancelPayment(paymentId), Times.Once);
    }

    [Fact]
    public async Task UpdatePayment_ServiceThrows_Returns500()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.CancelPayment(paymentId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentController.UpdatePayment(paymentId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        // Act
        var result = _paymentController.Health();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task CreatePayment_ZeroPrice_ReturnsPaymentId()
    {
        // Arrange
        var price = 0;
        var paymentId = Guid.NewGuid();
        
        _paymentServiceMock
            .Setup(x => x.CreatePayment(price))
            .ReturnsAsync(paymentId);

        // Act
        var result = await _paymentController.CreatePayment(price);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaymentIdDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var paymentIdDto = Assert.IsType<PaymentIdDto>(okResult.Value);
        
        Assert.Equal(paymentId, paymentIdDto.PaymentId);
        _paymentServiceMock.Verify(x => x.CreatePayment(price), Times.Once);
    }

    [Fact]
    public async Task UpdatePayment_CancelSuccess_ReturnsOk()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _paymentServiceMock.Setup(x => x.CancelPayment(paymentId)).ReturnsAsync(false);

        // Act
        var result = await _paymentController.UpdatePayment(paymentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _paymentServiceMock.Verify(x => x.CancelPayment(paymentId), Times.Once);
    }


    [Fact]
    public async Task UpdatePayment_ServiceError_Returns500()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _paymentServiceMock.Setup(x => x.CancelPayment(paymentId)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _paymentController.UpdatePayment(paymentId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}