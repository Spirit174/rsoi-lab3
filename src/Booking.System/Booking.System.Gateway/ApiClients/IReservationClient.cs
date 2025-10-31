using Booking.System.Gateway.DTO;

namespace Booking.System.Gateway.ApiClients;

/// <summary>
/// Клиент для взаимодействия с API сервиса бронирований.
/// </summary>
public interface IReservationClient
{
    /// <summary>
    /// Получает страницу отелей с пагинацией.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="size">Размер страницы.</param>
    /// <returns>Страница с отелями.</returns>
    Task<ServiceResponse<HotelPagesDto>> GetHotelsPageAsync(int page, int size);

    /// <summary>
    /// Получает отель по идентификатору.
    /// </summary>
    /// <param name="hotelId">Идентификатор отеля.</param>
    /// <returns>Информация об отеле.</returns>
    /// <exception cref="HotelNotFoundException">Выбрасывается, если отель не найден.</exception>
    Task<ServiceResponse<HotelDto>> GetHotelByIdAsync(Guid hotelId);

    /// <summary>
    /// Отменяет бронирование.
    /// </summary>
    /// <param name="reservationId">Идентификатор бронирования для отмены.</param>
    /// <exception cref="ReservationNotFoundException">Выбрасывается, если бронирование не найдено.</exception>
    Task<ServiceResponse<bool>> CancelReservation(Guid reservationId);

    /// <summary>
    /// Получает бронирование по идентификатору.
    /// </summary>
    /// <param name="reservationId">Идентификатор бронирования.</param>
    /// <returns>Информация о бронировании.</returns>
    /// <exception cref="ReservationNotFoundException">Выбрасывается, если бронирование не найдено.</exception>
    Task<ServiceResponse<ReservationDto>> GetReservationById(Guid reservationId);

    /// <summary>
    /// Создает новое бронирование.
    /// </summary>
    /// <param name="createReservationDto">Данные для создания бронирования.</param>
    Task<ServiceResponse<bool>> CreateReservation(CreateReservationDto createReservationDto);

    /// <summary>
    /// Получает список бронирований по имени пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <returns>Список бронирований пользователя.</returns>
    Task<ServiceResponse<List<ReservationDto>>> GetReservationByUsername(string userName);
}