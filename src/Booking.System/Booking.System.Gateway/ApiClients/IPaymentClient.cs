using Booking.System.Gateway.DTO;
using Booking.System.Gateway.Exceptions;

namespace Booking.System.Gateway.ApiClients;

/// <summary>
/// Клиент для взаимодействия с API платежной системы.
/// </summary>
public interface IPaymentClient
{
    /// <summary>
    /// Получает информацию о платеже по идентификатору.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа.</param>
    /// <returns>Информация о платеже.</returns>
    /// <exception cref="PaymentNotFoundException">Выбрасывается, если платеж не найден.</exception>
    Task<PaymentInfoDto> GetPaymentAsync(Guid paymentId);

    /// <summary>
    /// Обновляет информацию о платеже.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа для обновления.</param>
    /// <exception cref="PaymentNotFoundException">Выбрасывается, если платеж не найден.</exception>
    Task UpdatePaymentAsync(Guid paymentId);

    /// <summary>
    /// Создает новый платеж.
    /// </summary>
    /// <param name="price">Сумма платежа.</param>
    /// <returns>Идентификатор созданного платежа.</returns>
    Task<Guid> CreatePaymentAsync(int price);
}