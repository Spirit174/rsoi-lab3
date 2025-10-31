using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Models;
using Booking.System.ReservationService.DataBase.Context;
using Booking.System.ReservationService.DataBase.Converters;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.ReservationService.DataBase.Repositories;

public class HotelRepository: IHotelRepository
{
    private readonly ReservationContext _context;
    private readonly ILogger<HotelRepository> _logger;

    public HotelRepository(ReservationContext context,
        ILogger<HotelRepository> logger)
    {
        _context = context;
        _logger = logger;
    }    
    
    public async Task CreateHotelAsync(Hotel hotel)
    {
        _logger.LogDebug("Creating hotel with id: {HotelId}", hotel.HotelUid);
        
        await _context.Hotels.AddAsync(HotelConverter.Convert(hotel));
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created hotel with id: {HotelId}", hotel.HotelUid);
    }
    
    public async Task<Hotel?> GetHotelByHotelIdAsync(Guid hotelId)
    {
        _logger.LogDebug("Getting hotel with id: {HotelId}", hotelId);
        
        var dbHotel = await _context.Hotels.FirstOrDefaultAsync(p => p.HotelUid == hotelId);
        return HotelConverter.Convert(dbHotel);
    }
    
    public async Task<HotelPages> GetHotelByHotelsByPagesAsync(int page, int size)
    {
        var totalCount = await _context.Hotels.CountAsync();
            
        _logger.LogDebug("Total hotels count: {TotalCount}", totalCount);
        
        var skip = (page - 1) * size;
        
        var hotels = await _context.Hotels
            .Skip(skip)
            .Take(size)
            .ToListAsync();
        
        _logger.LogInformation("Getting hotels by page and size: {Page}:{Size}", page, size);

        var hotelPages = new HotelPages(hotels.ConvertAll(HotelConverter.Convert), page, size, totalCount);

        return hotelPages;
    }
}