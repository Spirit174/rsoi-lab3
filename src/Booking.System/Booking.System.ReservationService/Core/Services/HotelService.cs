using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Models;

namespace Booking.System.ReservationService.Core.Services;

public class HotelService: IHotelService
{
    private readonly ILogger<HotelService> _logger;
    private readonly IHotelRepository _hotelRepository;
    
    public HotelService(ILogger<HotelService> logger,
        IHotelRepository hotelRepository)
    {
        _logger = logger;
        _hotelRepository = hotelRepository;
    }
    
    public async Task<Hotel?> GetHotelByHotelIdAsync(Guid hotelId)
    {
        _logger.LogDebug("Getting hotel with id: {HotelId}", hotelId);
        
        return await _hotelRepository.GetHotelByHotelIdAsync(hotelId);
    }
    
    public async Task<HotelPages> GetHotelsByPagesAsync(int page, int size)
    {
        _logger.LogDebug("Getting hotels by page: {Page} and size: {Size}", page, size);
        
        var hotelPages = await _hotelRepository.GetHotelByHotelsByPagesAsync(page, size);
        
        _logger.LogInformation("Successfully retrieved {Size} hotels from page {Page}.", 
            size, page);
        
        return hotelPages;
    }
}