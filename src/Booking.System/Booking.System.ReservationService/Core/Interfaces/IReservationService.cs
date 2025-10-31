using Booking.System.ReservationService.Core.Models;

namespace Booking.System.ReservationService.Core.Interfaces;

/// <summary>
/// Сервис для работы с бронированиями.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Создает новое бронирование.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="paymentUid">Идентификатор платежа.</param>
    /// <param name="hotelUid">Идентификатор отеля.</param>
    /// <param name="startDate">Дата начала бронирования.</param>
    /// <param name="endDate">Дата окончания бронирования.</param>
    Task<Reservation> CreateReservationAsync(Guid reservationUid, string userName, Guid paymentUid, Guid hotelUid, DateTime startDate, DateTime endDate);

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