using Booking.System.ReservationService.Core.Models;

namespace Booking.System.ReservationService.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с данными о бронированиях.
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Создает новое бронирование.
    /// </summary>
    /// <param name="reservation">Объект бронирования для создания.</param>
    Task CreateReservationAsync(Reservation reservation);

    /// <summary>
    /// Получает бронирование по уникальному идентификатору.
    /// </summary>
    /// <param name="reservationUid">Уникальный идентификатор бронирования.</param>
    /// <returns>Объект бронирования или null, если не найден.</returns>
    Task<Reservation?> GetReservationByReservationIdAsync(Guid reservationUid);

    /// <summary>
    /// Получает список бронирований по имени пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <returns>Список бронирований пользователя.</returns>
    Task<List<Reservation?>> GetReservationByUserNameAsync(string userName);

    /// <summary>
    /// Отменяет бронирование.
    /// </summary>
    /// <param name="reservationUid">Уникальный идентификатор бронирования для отмены.</param>
    /// <returns>True - если бронирование найдено и отменено, False - если бронирование не найдено.</returns>
    Task<bool> CancelReservation(Guid reservationUid);
}