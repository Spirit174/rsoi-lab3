using Booking.System.Gateway.DTO;

namespace Booking.System.Gateway.Services;


public interface IGatewayService
{
    Task<ServiceResponse<HotelPagesDto>> GetHotelsAsync(int page, int size);
    Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync(string username);
    Task<ServiceResponse<List<ReservationDtoWithHotelAndPayment>>> GetUserReservationsAsync(string username);
    Task<ServiceResponse<ReservationDtoWithHotelAndPayment?>> GetReservationAsync(string username, Guid reservationUid);
    Task<ServiceResponse<CreateReservationResponse?>> CreateReservationAsync(string username, CreateReservationRequest request);
    Task<ServiceResponse<bool>> CancelReservationAsync(string username, Guid reservationUid);
    Task<ServiceResponse<LoyaltyInfoDto>> GetLoyaltyInfoAsync(string username);
}