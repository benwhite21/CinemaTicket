using CinemaTicket.Application.DTOs;

namespace CinemaTicket.Application.Interfaces
{
    public interface IBookingService
    {
        Task<SelectSeatsViewModel?> GetSelectSeatsViewModelAsync(int showtimeId);
        Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(CreateBookingDto dto, string userId);
        Task<BookingDto?> GetBookingByIdAsync(int id);
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId);
        Task<(bool Success, string Message)> CancelBookingAsync(int id, string userId);
        Task ExpireOldBookingsAsync();
    }
}