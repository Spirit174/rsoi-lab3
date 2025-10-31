using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.Core.Models.Enums;

namespace Booking.System.ReservationService.Core.Services;

public class ReservationService: IReservationService
{
    private readonly ILogger<ReservationService> _logger;
    private readonly IReservationRepository _reservationRepository;
    
    public ReservationService(ILogger<ReservationService> logger,
        IReservationRepository reservationRepository)
    {
        _logger = logger;
        _reservationRepository = reservationRepository;
    }
    
    public async Task<Reservation> CreateReservationAsync(Guid reservationUid, string userName, Guid paymentUid, Guid hotelUid, DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Creating reservation for user with username: {UserName}", userName);
        var reservation = new Reservation(default, reservationUid, userName, paymentUid, hotelUid,
            PaymentStatus.PAID, startDate, endDate);
        await _reservationRepository.CreateReservationAsync(reservation);
        _logger.LogInformation("Successfully created reservation with id: {ReservationUid}", reservation.ReservationUid);
        return reservation;
    }
   
    
    public async Task<Reservation?> GetReservationByReservationIdAsync(Guid reservationUid)
    {
        return await _reservationRepository.GetReservationByReservationIdAsync(reservationUid);
    }
    
    public async Task<List<Reservation?>> GetReservationByUserNameAsync(string userName)
    {
        _logger.LogDebug("Getting reservations for user: {UserName}", userName);
        
        var reservations = await _reservationRepository.GetReservationByUserNameAsync(userName);
        
        _logger.LogInformation("Successfully retrieved {ReservationsCount} reservations for user: {UserName}", 
            reservations.Count, userName);
        
        return reservations;
    }
    
    public async Task<bool> CancelReservation(Guid reservationUid)
    {
        _logger.LogDebug("Canceling reservation with id: {ReservationUid}", reservationUid);
        if (await _reservationRepository.CancelReservation(reservationUid))
        {
            _logger.LogInformation("Successfully canceled reservation with id: {ReservationUid}", reservationUid);
            return true;
        }
        else
        {
            _logger.LogWarning("Reservation with id: {ReservationUid} not found for cancellation", reservationUid);
            return false;
        }
    }
}