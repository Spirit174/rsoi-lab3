using Booking.System.LoyaltyService.Core.Interfaces;
using Booking.System.LoyaltyService.Core.Models;
using Booking.System.LoyaltyService.DataBase.Context;
using Booking.System.LoyaltyService.DataBase.Converters;
using Booking.System.LoyaltyService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.LoyaltyService.DataBase.Repositories;

public class LoyaltyRepostirory : ILoyaltyRepository
{
    private readonly LoyaltyContext _context;
    private readonly ILogger<LoyaltyRepostirory> _logger;

    public LoyaltyRepostirory(LoyaltyContext context, ILogger<LoyaltyRepostirory> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task CreateLoyaltyAsync(Loyalty loyalty)
    {
        _logger.LogDebug("Creating loyalty for user: {UserName}", loyalty.Username);
        
        await _context.Loyalties.AddAsync(LoyaltyConverter.Convert(loyalty));
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created loyalty for user: {UserName}", loyalty.Username);
    }

    public async Task UpdateLoyaltyReservationCountDecreaseAsync(string userName)
    {
        _logger.LogDebug("Decreasing reservation count for user: {UserName}", userName);
        
        var dbLoyalty = await _context.Loyalties.FirstOrDefaultAsync(l => l.Username == userName);

        var oldCount = dbLoyalty.ReservationCount;
        var oldStatus = dbLoyalty.Status;
        
        dbLoyalty.ReservationCount -= 1;
        
        if (dbLoyalty.ReservationCount < 10 && dbLoyalty.Status is LoyaltyStatus.SILVER)
        {
            dbLoyalty.Status = LoyaltyStatus.BRONZE;
            dbLoyalty.Discount = 5;
            _logger.LogInformation("User {UserName} status changed from SILVER to BRONZE. Reservation count: {NewCount}", 
                userName, dbLoyalty.ReservationCount);
        }
        
        if (dbLoyalty.ReservationCount < 20 && dbLoyalty.ReservationCount >= 10 && dbLoyalty.Status is LoyaltyStatus.GOLD)
        {
            dbLoyalty.Status = LoyaltyStatus.SILVER;
            dbLoyalty.Discount = 7;
            _logger.LogInformation("User {UserName} status changed from GOLD to SILVER. Reservation count: {NewCount}", 
                userName, dbLoyalty.ReservationCount);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Reservation count decreased for user {UserName}. Count: {OldCount} -> {NewCount}, Status: {OldStatus} -> {NewStatus}", 
            userName, oldCount, dbLoyalty.ReservationCount, oldStatus, dbLoyalty.Status);
    }
    
    public async Task UpdateLoyaltyReservationCountIncreaseAsync(string userName)
    {
        _logger.LogDebug("Increasing reservation count for user: {UserName}", userName);
        
        var dbLoyalty = await _context.Loyalties.FirstOrDefaultAsync(l => l.Username == userName);

        var oldCount = dbLoyalty.ReservationCount;
        var oldStatus = dbLoyalty.Status;
        
        dbLoyalty.ReservationCount += 1;
        
        if (dbLoyalty.ReservationCount >= 10 && dbLoyalty.Status is not LoyaltyStatus.SILVER)
        {
            dbLoyalty.Status = LoyaltyStatus.SILVER;
            dbLoyalty.Discount = 7;
            _logger.LogInformation("User {UserName} status changed to SILVER. Reservation count: {NewCount}", 
                userName, dbLoyalty.ReservationCount);
        }
        
        if (dbLoyalty.ReservationCount >= 20 && dbLoyalty.Status is not LoyaltyStatus.GOLD)
        {
            dbLoyalty.Status = LoyaltyStatus.GOLD;
            dbLoyalty.Discount = 10;
            _logger.LogInformation("User {UserName} status changed to GOLD. Reservation count: {NewCount}", 
                userName, dbLoyalty.ReservationCount);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Reservation count increased for user {UserName}. Count: {OldCount} -> {NewCount}, Status: {OldStatus} -> {NewStatus}", 
            userName, oldCount, dbLoyalty.ReservationCount, oldStatus, dbLoyalty.Status);
    }

    public async Task<Loyalty?> GetLoyaltyByUserNameAsync(string userName)
    {
        _logger.LogDebug("Getting loyalty for user: {UserName}", userName);
        
        var dbLoyalty = await _context.Loyalties.FirstOrDefaultAsync(l => l.Username == userName);

        _logger.LogDebug("Loyalty found for user: {UserName}. Count: {Count}, Status: {Status}", 
            userName, dbLoyalty.ReservationCount, dbLoyalty.Status);
        
        return LoyaltyConverter.Convert(dbLoyalty);
    }
}