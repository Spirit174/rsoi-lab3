using Booking.System.ReservationService.Core.Models;

namespace Booking.System.ReservationService.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с данными об отелях.
/// </summary>
public interface IHotelRepository
{
    /// <summary>
    /// Создает новый отель.
    /// </summary>
    /// <param name="hotel">Отель для создания.</param>
    Task CreateHotelAsync(Hotel hotel);

    /// <summary>
    /// Получить отель по идентификатору отеля.
    /// </summary>
    /// <param name="hotelId">Идентификатор отеля.</param>
    /// <returns>Объект отеля или null.</returns>
    Task<Hotel?> GetHotelByHotelIdAsync(Guid hotelId);

    /// <summary>
    /// Получает список отелей с пагинацией.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="size">Размер страницы.</param>
    /// <returns>Список отелей и общее количество.</returns>
    Task<HotelPages> GetHotelByHotelsByPagesAsync(int page, int size);
}