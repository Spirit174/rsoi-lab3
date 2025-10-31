using Booking.System.LoyaltyService.Core.Models;

namespace Booking.System.LoyaltyService.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с данными о лояльности.
/// </summary>
public interface ILoyaltyRepository
{
    /// <summary>
    /// Создает новую запись о лояльности.
    /// </summary>
    /// <param name="loyalty">Объект лояльности для создания.</param>
    Task CreateLoyaltyAsync(Loyalty loyalty);

    /// <summary>
    /// Уменьшает счетчик бронирований и обновляет статус лояльности.
    /// </summary>
    /// <param name="userName">Имя пользователя для обновления.</param>
    Task UpdateLoyaltyReservationCountDecreaseAsync(string userName);

    /// <summary>
    /// Увеличивает счетчик бронирований и обновляет статус лояльности.
    /// </summary>
    /// <param name="userName">Имя пользователя для обновления.</param>
    Task UpdateLoyaltyReservationCountIncreaseAsync(string userName);

    /// <summary>
    /// Получает информацию о лояльности по имени пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя для поиска.</param>
    /// <returns>Найденный объект лояльности или null.</returns>
    Task<Loyalty?> GetLoyaltyByUserNameAsync(string userName);
}