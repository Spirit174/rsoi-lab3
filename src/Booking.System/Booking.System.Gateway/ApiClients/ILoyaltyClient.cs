using Booking.System.Gateway.DTO;

namespace Booking.System.Gateway.ApiClients;

/// <summary>
/// Клиент для взаимодействия с API системы лояльности.
/// </summary>
public interface ILoyaltyClient
{
    /// <summary>
    /// Получает информацию о лояльности пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя для получения информации о лояльности.</param>
    /// <returns>Информация о лояльности пользователя.</returns>
    Task<ServiceResponse<LoyaltyInfoDto>> GetLoyaltyAsync(string userName);

    /// <summary>
    /// Обновляет счетчик бронирований пользователя.
    /// </summary>
    /// <param name="userName">Имя пользователя для обновления.</param>
    /// <param name="isIncrease">True - увеличить счетчик, False - уменьшить счетчик.</param>
    Task<ServiceResponse<bool>> UpdateLoyaltyReservationCountAsync(string userName, bool isIncrease);
}