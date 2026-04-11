using CinemaTicket.Application.DTOs;

namespace CinemaTicket.Application.Interfaces
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieDto>> GetAllAsync(string? search = null, int? genreId = null, string? status = null);
        Task<MovieDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(CreateMovieDto dto);
        Task<(bool Success, string Message)> UpdateAsync(EditMovieDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<IEnumerable<MovieDto>> GetNowShowingAsync();
        Task<IEnumerable<MovieDto>> GetComingSoonAsync();
    }
}