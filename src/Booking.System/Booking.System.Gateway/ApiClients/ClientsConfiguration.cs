namespace Booking.System.Gateway.ApiClients;

/// <summary>
/// Конфигурация апи сервисов.
/// </summary>
public class ClientsConfiguration
{
    /// <summary>
    /// Url до Loyalty APi.
    /// </summary>
    public string? UrlLoyalty { get; set; }
    
    /// <summary>
    /// Url до Payment APi.
    /// </summary>
    public string? UrlPayment { get; set; }
    
    /// <summary>
    /// Url до Reservation APi.
    /// </summary>
    public string? UrlReservation { get; set; }
}