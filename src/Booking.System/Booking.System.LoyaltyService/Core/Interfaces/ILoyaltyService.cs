using Booking.System.LoyaltyService.Core.Models;

namespace Booking.System.LoyaltyService.Core.Interfaces;

/// <summary>
/// Сервис для работы с системой лояльности.
/// </summary>
public interface ILoyaltyService
{
    /// <summary>
    /// Получает информацию о лояльности пользователя, создавая новую запись если не найдена.
    /// </summary>
    /// <param name="userName">Имя пользователя для поиска или создания.</param>
    /// <returns>Объект лояльности пользователя.</returns>
    Task<Loyalty> GetLoyaltyAndCreateIfNotExist(string userName);

    /// <summary>
    /// Обновляет счетчик бронирований пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя для обновления.</param>
    /// <param name="isIncrease">True - увеличить счетчик, False - уменьшить счетчик.</param>
    Task UpdateLoyalty(string userName, bool isIncrease);
}