using Booking.System.PaymentService.Core.Models;

namespace Booking.System.PaymentService.Core.Interfaces;

/// <summary>
/// Сервис для работы с платежами.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Создает новый платеж.
    /// </summary>
    /// <param name="price">Сумма платежа.</param>
    /// <returns>Идентификатор созданного платежа.</returns>
    Task<Guid> CreatePayment(int price);

    /// <summary>
    /// Отменяет платеж.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа для отмены.</param>
    /// <returns>True - если платеж отменен, False - если платеж не найден.</returns>
    Task<bool> CancelPayment(Guid paymentId);

    /// <summary>
    /// Получить платеж по идентификатору платежа.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа для отмены.</param>
    /// <returns>Объект платежа или null.</returns>
    Task<Payment?> GetPayment(Guid paymentId);
}