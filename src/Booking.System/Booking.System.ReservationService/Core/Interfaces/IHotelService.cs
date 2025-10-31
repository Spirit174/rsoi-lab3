using Booking.System.ReservationService.Core.Models;

namespace Booking.System.ReservationService.Core.Interfaces;

/// <summary>
/// Сервис для работы с отелями.
/// </summary>
public interface IHotelService
{
    /// <summary>
    /// Получает отель по идентификатору.
    /// </summary>
    /// <param name="hotelId">Идентификатор отеля.</param>
    /// <returns>Объект отеля или null, если не найден.</returns>
    Task<Hotel?> GetHotelByHotelIdAsync(Guid hotelId);

    /// <summary>
    /// Получает список отелей с пагинацией.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="size">Размер страницы.</param>
    /// <returns>Объект с информацией о странице отелей.</returns>
    Task<HotelPages> GetHotelsByPagesAsync(int page, int size);
}