using Booking.System.LoyaltyService.Core.Interfaces;
using Booking.System.LoyaltyService.DTO.Converters;
using Booking.System.LoyaltyService.DTO.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking.System.LoyaltyService.Controllers;

[ApiController]
[Route("/api/v1")]
public class LoyaltyController: ControllerBase
{
    private readonly ILogger<LoyaltyController> _logger;
    private readonly ILoyaltyService _loyaltyService;
    
    public LoyaltyController(ILogger<LoyaltyController> logger,
        ILoyaltyService loyaltyService)
    {
        _logger = logger;
        _loyaltyService = loyaltyService;
    }
    
    /// <summary>
    /// Получить информацию о статусе в программе лояльности.
    /// </summary>
    [HttpGet("loyalty/{userName}")]
    public async Task<ActionResult<LoyaltyInfoDto>> GetLoyaltyInfo([FromRoute] string userName)
    {
        try
        {
            _logger.LogInformation($"GetLoyaltyInfo: {userName}");
            var loyalty = await _loyaltyService.GetLoyaltyAndCreateIfNotExist(userName);

            return Ok(LoyaltyInfoDtoConverter.Convert(loyalty));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in loyalty service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    
    /// <summary>
    /// Обновить после бронирования или отмены бронирования.
    /// </summary>
    [HttpPost("loyalty/{userName}")]
    public async Task<ActionResult> UpdateLoyalty([FromRoute] string userName, [FromBody] IncreaseBool isIncreaseBool)
    {
        try
        {
            _logger.LogInformation($"UpdateLoyalty: {userName}");
            var loyalty = await _loyaltyService.GetLoyaltyAndCreateIfNotExist(userName);

            await _loyaltyService.UpdateLoyalty(loyalty.Username, isIncreaseBool.IsIncrease);

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while processing request in loyalty service");

            return StatusCode(500, new ErrorResponse("Неожиданная ошибка на стороне сервера."));
        }
    }
    
    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }
}