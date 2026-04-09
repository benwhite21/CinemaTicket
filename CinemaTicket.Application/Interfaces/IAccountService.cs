using CinemaTicket.Application.DTOs;

namespace CinemaTicket.Application.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto dto);
        Task LogoutAsync();
        Task<UserDto?> GetCurrentUserAsync(string userId);
    }
}