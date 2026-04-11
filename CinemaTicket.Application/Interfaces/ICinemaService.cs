using CinemaTicket.Application.DTOs;

namespace CinemaTicket.Application.Interfaces
{
    public interface ICinemaService
    {
        Task<IEnumerable<CinemaDto>> GetAllCinemasAsync();
        Task<CinemaDto?> GetCinemaByIdAsync(int id);
        Task<(bool Success, string Message)> CreateCinemaAsync(CreateCinemaDto dto);
        Task<(bool Success, string Message)> DeleteCinemaAsync(int id);

        Task<IEnumerable<HallDto>> GetHallsByCinemaAsync(int cinemaId);
        Task<HallDto?> GetHallByIdAsync(int id);
        Task<(bool Success, string Message)> CreateHallAsync(CreateHallDto dto);
        Task<IEnumerable<SeatDto>> GetSeatsByHallAsync(int hallId);
    }
}