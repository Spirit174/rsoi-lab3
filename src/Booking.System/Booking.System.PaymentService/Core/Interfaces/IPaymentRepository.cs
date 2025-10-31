using Booking.System.PaymentService.Core.Models;

namespace Booking.System.PaymentService.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с данными о платежах.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Создает новый платеж.
    /// </summary>
    /// <param name="payment">Объект платежа для создания.</param>
    Task CreatePaymentAsync(Payment payment);

    /// <summary>
    /// Отменяет платеж по идентификатору.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа для отмены.</param>
    /// <returns>True - если платеж найден и отменен, False - если платеж не найден.</returns>
    Task<bool> UpdatePaymentAsync(Guid paymentId);

    /// <summary>
    /// Получить платеж по идентификатору платежа.
    /// </summary>
    /// <param name="paymentId">Идентификатор платежа для отмены.</param>
    /// <returns>Объект платежа или null.</returns>
    Task<Payment?> GetPaymentByPaymentIdAsync(Guid paymentId);
}