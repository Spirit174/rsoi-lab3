using Booking.System.LoyaltyService.Controllers;
using Booking.System.LoyaltyService.Core.Interfaces;
using Booking.System.LoyaltyService.Core.Models;
using Booking.System.LoyaltyService.Core.Models.Enums;
using Booking.System.LoyaltyService.DTO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests;

public class LoyaltyServiceTests : IDisposable
{
    private readonly Mock<ILogger<LoyaltyController>> _loggerMock;
    private readonly Mock<ILoyaltyService> _loyaltyServiceMock;
    private readonly LoyaltyController _loyaltyController;

    public LoyaltyServiceTests()
    {
        _loggerMock = new Mock<ILogger<LoyaltyController>>();
        _loyaltyServiceMock = new Mock<ILoyaltyService>();
        
        _loyaltyController = new LoyaltyController(_loggerMock.Object, _loyaltyServiceMock.Object);
    }

    public void Dispose()
    {
        
    }

    [Fact]
    public async Task GetLoyaltyInfo_ExistingUser_ReturnsOkWithCorrectDto()
    {
        // Arrange
        var userName = "testUser";
        var loyalty = new Loyalty(
            id: 1,
            username: userName,
            reservationCount: 10,
            status: LoyaltyStatus.SILVER,
            discount: 7
        );

        _loyaltyServiceMock
            .Setup(service => service.GetLoyaltyAndCreateIfNotExist(userName))
            .ReturnsAsync(loyalty);

        // Act
        var result = await _loyaltyController.GetLoyaltyInfo(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoyaltyInfoDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var loyaltyInfoDto = Assert.IsType<LoyaltyInfoDto>(okResult.Value);
        
        // Проверяем, что DTO содержит правильные значения (после конвертации)
        Assert.Equal("SILVER", loyaltyInfoDto.Status); // Enum конвертируется в string
        Assert.Equal(7, loyaltyInfoDto.Discount);
        Assert.Equal(10, loyaltyInfoDto.ReservationCount);
        
        _loyaltyServiceMock.Verify(
            service => service.GetLoyaltyAndCreateIfNotExist(userName), 
            Times.Once);
    }

    [Fact]
    public async Task GetLoyaltyInfo_BronzeStatus_ReturnsCorrectDto()
    {
        // Arrange
        var userName = "bronzeUser";
        var loyalty = new Loyalty(
            id: 1,
            username: userName,
            reservationCount: 5,
            status: LoyaltyStatus.BRONZE,
            discount: 5
        );

        _loyaltyServiceMock
            .Setup(service => service.GetLoyaltyAndCreateIfNotExist(userName))
            .ReturnsAsync(loyalty);

        // Act
        var result = await _loyaltyController.GetLoyaltyInfo(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoyaltyInfoDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var loyaltyInfoDto = Assert.IsType<LoyaltyInfoDto>(okResult.Value);
        
        Assert.Equal("BRONZE", loyaltyInfoDto.Status);
        Assert.Equal(5, loyaltyInfoDto.Discount);
        Assert.Equal(5, loyaltyInfoDto.ReservationCount);
    }

    [Fact]
    public async Task GetLoyaltyInfo_GoldStatus_ReturnsCorrectDto()
    {
        // Arrange
        var userName = "goldUser";
        var loyalty = new Loyalty(
            id: 1,
            username: userName,
            reservationCount: 25,
            status: LoyaltyStatus.GOLD,
            discount: 10
        );

        _loyaltyServiceMock
            .Setup(service => service.GetLoyaltyAndCreateIfNotExist(userName))
            .ReturnsAsync(loyalty);

        // Act
        var result = await _loyaltyController.GetLoyaltyInfo(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoyaltyInfoDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var loyaltyInfoDto = Assert.IsType<LoyaltyInfoDto>(okResult.Value);
        
        Assert.Equal("GOLD", loyaltyInfoDto.Status);
        Assert.Equal(10, loyaltyInfoDto.Discount);
        Assert.Equal(25, loyaltyInfoDto.ReservationCount);
    }

    [Fact]
    public async Task GetLoyaltyInfo_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var userName = "testUser";
        
        _loyaltyServiceMock
            .Setup(service => service.GetLoyaltyAndCreateIfNotExist(userName))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _loyaltyController.GetLoyaltyInfo(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoyaltyInfoDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", errorResponse.Message);
        
        _loyaltyServiceMock.Verify(
            service => service.GetLoyaltyAndCreateIfNotExist(userName), 
            Times.Once);
        
        // Verify that exception was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        // Act
        var result = _loyaltyController.Health();

        // Assert
        var actionResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }
    
    [Fact]
    public async Task UpdateLoyalty_IncreaseTrue_CallsServiceWithTrue()
    {
        // Arrange
        var userName = "testUser";
        var increaseBool = new IncreaseBool(true);
        var loyalty = new Loyalty(1, userName, 5, LoyaltyStatus.BRONZE, 5);

        _loyaltyServiceMock.Setup(x => x.GetLoyaltyAndCreateIfNotExist(userName)).ReturnsAsync(loyalty);

        // Act
        var result = await _loyaltyController.UpdateLoyalty(userName, increaseBool);

        // Assert
        Assert.IsType<OkResult>(result);
        _loyaltyServiceMock.Verify(x => x.UpdateLoyalty(userName, true), Times.Once);
    }

    [Fact]
    public async Task UpdateLoyalty_IncreaseFalse_CallsServiceWithFalse()
    {
        // Arrange
        var userName = "testUser";
        var increaseBool = new IncreaseBool(false);
        var loyalty = new Loyalty(1, userName, 5, LoyaltyStatus.BRONZE, 5);

        _loyaltyServiceMock.Setup(x => x.GetLoyaltyAndCreateIfNotExist(userName)).ReturnsAsync(loyalty);

        // Act
        var result = await _loyaltyController.UpdateLoyalty(userName, increaseBool);

        // Assert
        Assert.IsType<OkResult>(result);
        _loyaltyServiceMock.Verify(x => x.UpdateLoyalty(userName, false), Times.Once);
    }

    [Fact]
    public async Task UpdateLoyalty_UserNotExists_CreatesUserAndUpdates()
    {
        // Arrange
        var userName = "newUser";
        var increaseBool = new IncreaseBool(true);
        var newLoyalty = new Loyalty(1, userName, 0, LoyaltyStatus.BRONZE, 0);

        _loyaltyServiceMock.Setup(x => x.GetLoyaltyAndCreateIfNotExist(userName)).ReturnsAsync(newLoyalty);

        // Act
        var result = await _loyaltyController.UpdateLoyalty(userName, increaseBool);

        // Assert
        Assert.IsType<OkResult>(result);
        _loyaltyServiceMock.Verify(x => x.GetLoyaltyAndCreateIfNotExist(userName), Times.Once);
        _loyaltyServiceMock.Verify(x => x.UpdateLoyalty(userName, true), Times.Once);
    }

    [Fact]
    public async Task UpdateLoyalty_ServiceThrows_Returns500()
    {
        // Arrange
        var userName = "testUser";
        var increaseBool = new IncreaseBool(true);

        _loyaltyServiceMock.Setup(x => x.GetLoyaltyAndCreateIfNotExist(userName))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _loyaltyController.UpdateLoyalty(userName, increaseBool);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        
        var error = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("Неожиданная ошибка на стороне сервера.", error.Message);
    }

    [Fact]
    public async Task GetLoyaltyInfo_NewUser_CreatesLoyaltyRecord()
    {
        // Arrange
        var userName = "newUser";
        var newLoyalty = new Loyalty(
            id: 1,
            username: userName,
            reservationCount: 0,
            status: LoyaltyStatus.BRONZE,
            discount: 0
        );

        _loyaltyServiceMock
            .Setup(service => service.GetLoyaltyAndCreateIfNotExist(userName))
            .ReturnsAsync(newLoyalty);

        // Act
        var result = await _loyaltyController.GetLoyaltyInfo(userName);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoyaltyInfoDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var loyaltyInfoDto = Assert.IsType<LoyaltyInfoDto>(okResult.Value);
        
        Assert.Equal("BRONZE", loyaltyInfoDto.Status);
        Assert.Equal(0, loyaltyInfoDto.Discount);
        Assert.Equal(0, loyaltyInfoDto.ReservationCount);
        
        _loyaltyServiceMock.Verify(
            service => service.GetLoyaltyAndCreateIfNotExist(userName), 
            Times.Once);
    }
}