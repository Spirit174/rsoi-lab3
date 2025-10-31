using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DataBase.Context;
using Booking.System.ReservationService.DataBase.Converters;
using Booking.System.ReservationService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.ReservationService.DataBase.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationContext _context;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(ReservationContext context,
        ILogger<ReservationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }    
    
    public async Task CreateReservationAsync(Reservation reservation)
    {
        _logger.LogDebug("Creating reservation with id: {ReservationUid}", reservation.ReservationUid);
        var dbHotel = await _context.Hotels.FirstOrDefaultAsync(p => p.HotelUid == reservation.HotelUid);
        await _context.Reservations.AddAsync(ReservationConverter.Convert(reservation, dbHotel!));
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created reservation with id: {ReservationUid}", reservation.ReservationUid);
    }
    
    public async Task<Reservation?> GetReservationByReservationIdAsync(Guid reservationUid)
    {
        _logger.LogDebug("Getting reservation with id: {HotelId}", reservationUid);
        
        var dbReservation = await _context.Reservations.FirstOrDefaultAsync(p => p.ReservationUid == reservationUid);
        if (dbReservation == null)
            return null;
        var dbHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == dbReservation.HotelId);
        if (dbHotel == null)
            return null;
        return ReservationConverter.Convert(dbReservation, dbHotel.HotelUid);
    }
    
    public async Task<List<Reservation?>> GetReservationByUserNameAsync(string userName)
    {
        _logger.LogDebug("Getting reservations with username: {UserName}", userName);
    
        var dbReservations = await _context.Reservations
            .Where(p => p.Username == userName)
            .ToListAsync();

        var result = new List<Reservation?>();
    
        foreach (var dbReservation in dbReservations)
        {
            var dbHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == dbReservation.HotelId);
            if (dbHotel != null)
            {
                result.Add(ReservationConverter.Convert(dbReservation, dbHotel.HotelUid));
            }
        }
    
        return result;
    }
    
    public async Task<bool> CancelReservation(Guid reservationUid)
    {
        _logger.LogDebug("Getting reservations with id: {ReservationUid}", reservationUid);

        var dbReservation = await _context.Reservations.FirstOrDefaultAsync(p => p.ReservationUid == reservationUid);
        if (dbReservation is null)
            return false;
        
        _logger.LogDebug("Cancel reservation with id: {ReservationUid}", reservationUid);
        
        dbReservation.Status = DbPaymentStatus.CANCELED;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully Canceled reservation with id: {ReservationUid}", reservationUid);
        return true;
    }
}