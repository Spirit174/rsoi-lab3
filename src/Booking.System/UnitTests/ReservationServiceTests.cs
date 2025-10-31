using Booking.System.ReservationService.Controllers;
using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DTO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests;

public class ReservationServiceTests
{
    private readonly Mock<ILogger<ReservationController>> _loggerMock;
    private readonly Mock<IReservationService> _reservationServiceMock;
    private readonly ReservationController _reservationController;
    private readonly Mock<ILogger<HotelController>> _loggerHotelMock;
    private readonly Mock<IHotelService> _hotelServiceMock;
    private readonly HotelController _hotelController;

    public ReservationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ReservationController>>();
        _reservationServiceMock = new Mock<IReservationService>();
        _reservationController = new ReservationController(_reservationServiceMock.Object, _loggerMock.Object);
        _loggerHotelMock = new Mock<ILogger<HotelController>>();
        _hotelServiceMock = new Mock<IHotelService>();
        _hotelController = new HotelController(_hotelServiceMock.Object, _loggerHotelMock.Object);
    }

    [Fact]
    public async Task CancelReservation_SuccessfulCancel_ReturnsOk()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        
        _reservationServiceMock
            .Setup(x => x.CancelReservation(reservationId))
            .ReturnsAsync(true);

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        Assert.IsType<OkResult>(result);
        _reservationServiceMock.Verify(x => x.CancelReservation(reservationId), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_ReservationNotFound_ReturnsNotFound()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        
        _reservationServiceMock
            .Setup(x => x.CancelReservation(reservationId))
            .ReturnsAsync(false);

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var error = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        
        Assert.Equal("Нeт такой брони.", error.Message);
        _reservationServiceMock.Verify(x => x.CancelReservation(reservationId), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_ServiceThrows_Returns500()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        
        _reservationServiceMock
            .Setup(x => x.CancelReservation(reservationId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }
    [Fact]
    public async Task GetReservationId_NonExistingReservation_ReturnsNotFound()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        
        _reservationServiceMock
            .Setup(x => x.GetReservationByReservationIdAsync(reservationId))
            .ReturnsAsync((Reservation)null);

        // Act
        var result = await _reservationController.GetReservationId(reservationId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReservationDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var error = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        
        Assert.Equal("Нeт такой брони.", error.Message);
    }

    [Fact]
    public async Task CreateReservation_ServiceThrows_Returns500()
    {
        // Arrange
        var createReservationDto = new CreateReservationDto(
            Guid.NewGuid(),
            "testUser",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(5)
        );

        _reservationServiceMock
            .Setup(x => x.CreateReservationAsync(
                createReservationDto.ReservationUid,
                createReservationDto.Username,
                createReservationDto.PaymentUid,
                createReservationDto.HotelUid,
                createReservationDto.StartDate,
                createReservationDto.EndDate))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _reservationController.CreateReservation(createReservationDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public async Task GetReservationsById_ServiceThrows_Returns500()
    {
        // Arrange
        var userName = "testUser";
        
        _reservationServiceMock
            .Setup(x => x.GetReservationByUserNameAsync(userName))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _reservationController.GetReservationsById(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<ReservationDto>>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        // Act
        var result = _reservationController.Health();

        // Assert
        Assert.IsType<OkResult>(result);
    }
    
    [Fact]
    public async Task CancelReservation_Success_ReturnsOk()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationServiceMock.Setup(x => x.CancelReservation(reservationId)).ReturnsAsync(true);

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        Assert.IsType<OkResult>(result);
        _reservationServiceMock.Verify(x => x.CancelReservation(reservationId), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_NotFound_ReturnsNotFound()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationServiceMock.Setup(x => x.CancelReservation(reservationId)).ReturnsAsync(false);

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var error = Assert.IsType<ErrorResponse>(notFound.Value);
        Assert.Equal("Нeт такой брони.", error.Message);
    }

    [Fact]
    public async Task CancelReservation_ServiceError_Returns500()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        _reservationServiceMock.Setup(x => x.CancelReservation(reservationId)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _reservationController.CancelReservation(reservationId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
    [Fact]
    public async Task GetHotelsPages_ServiceThrows_Returns500()
    {
        // Arrange
        var page = 1;
        var size = 10;
        
        _hotelServiceMock
            .Setup(x => x.GetHotelsByPagesAsync(page, size))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _hotelController.GetHotelsPages(page, size);

        // Assert
        var actionResult = Assert.IsType<ActionResult<HotelPagesDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public async Task GetHotelById_NonExistingHotel_ReturnsBadRequest()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        
        _hotelServiceMock
            .Setup(x => x.GetHotelByHotelIdAsync(hotelId))
            .ReturnsAsync((Hotel)null);

        // Act
        var result = await _hotelController.GetHotelById(hotelId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<HotelDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Нeт такого отеля.", error.Message);
    }

    [Fact]
    public async Task GetHotelById_ServiceThrows_Returns500()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        
        _hotelServiceMock
            .Setup(x => x.GetHotelByHotelIdAsync(hotelId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _hotelController.GetHotelById(hotelId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<HotelDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }
}