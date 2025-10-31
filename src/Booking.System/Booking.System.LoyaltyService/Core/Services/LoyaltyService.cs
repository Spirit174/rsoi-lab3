using Booking.System.LoyaltyService.Core.Interfaces;
using Booking.System.LoyaltyService.Core.Models;
using Booking.System.LoyaltyService.Core.Models.Enums;

namespace Booking.System.LoyaltyService.Core.Services;

public class LoyaltyService : ILoyaltyService
{
    public readonly ILogger<LoyaltyService> _logger;
    public readonly ILoyaltyRepository _loyaltyRepository;
    
    public LoyaltyService(ILogger<LoyaltyService> logger,
        ILoyaltyRepository loyaltyRepository)
    {
        _logger = logger;
        _loyaltyRepository = loyaltyRepository;
    }
    
    public async Task<Loyalty> GetLoyaltyAndCreateIfNotExist(string userName)
    {
        _logger.LogDebug("Getting loyalty for user: {UserName}", userName);
        
        var loyalty = await _loyaltyRepository.GetLoyaltyByUserNameAsync(userName);
        
        if (loyalty is null)
        {
            _logger.LogDebug("Loyalty not found for user {UserName}. Creating new loyalty record.", userName);
            
            var newLoyalty = new Loyalty(1, userName, 0, LoyaltyStatus.BRONZE, 5);
            await _loyaltyRepository.CreateLoyaltyAsync(newLoyalty);
            
            _logger.LogInformation("Successfully created new loyalty for user {UserName}. ID: {LoyaltyId}, Status: {Status}", 
                userName, newLoyalty.Id, newLoyalty.Status);
            
            return newLoyalty;
        }
        
        _logger.LogInformation("Loyalty found for user {UserName}. Reservation count: {Count}, Status: {Status}", 
            userName, loyalty.ReservationCount, loyalty.Status);
        
        return loyalty;
    }
    
    public async Task UpdateLoyalty(string userName, bool isIncrease)
    {
        _logger.LogInformation("Updating loyalty for user {UserName}. Operation: {Operation}", 
            userName, isIncrease ? "Increase" : "Decrease");
        
        if (isIncrease)
        {
            _logger.LogDebug("Increasing reservation count for user: {UserName}", userName);
            await _loyaltyRepository.UpdateLoyaltyReservationCountIncreaseAsync(userName);
        }
        else
        {
            _logger.LogDebug("Decreasing reservation count for user: {UserName}", userName);
            await _loyaltyRepository.UpdateLoyaltyReservationCountDecreaseAsync(userName);
        }
        
        _logger.LogInformation("Successfully updated loyalty for user {UserName}. Operation: {Operation}",
            userName, isIncrease ? "Increase" : "Decrease");
    }
}