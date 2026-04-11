using CinemaTicket.Application.DTOs;

namespace CinemaTicket.Application.Interfaces
{
    public interface IShowtimeService
    {
        Task<IEnumerable<ShowtimeDto>> GetAllAsync(int? movieId = null, int? cinemaId = null, DateTime? date = null);
        Task<ShowtimeDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(CreateShowtimeDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<IEnumerable<ShowtimeDto>> GetByMovieAsync(int movieId, DateTime? date = null);
    }
}