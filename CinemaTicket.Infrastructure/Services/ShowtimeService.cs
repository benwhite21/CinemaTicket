using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Infrastructure.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly ApplicationDbContext _context;

        public ShowtimeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShowtimeDto>> GetAllAsync(int? movieId = null, int? cinemaId = null, DateTime? date = null)
        {
            var query = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .Include(s => s.ShowtimeSeats)
                .Where(s => s.IsActive)
                .AsQueryable();

            if (movieId.HasValue)
                query = query.Where(s => s.MovieId == movieId);

            if (cinemaId.HasValue)
                query = query.Where(s => s.Hall.CinemaId == cinemaId);

            if (date.HasValue)
                query = query.Where(s => s.StartTime.Date == date.Value.Date);

            return await query.OrderBy(s => s.StartTime)
                .Select(s => ToDto(s)).ToListAsync();
        }

        public async Task<ShowtimeDto?> GetByIdAsync(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .Include(s => s.ShowtimeSeats)
                .FirstOrDefaultAsync(s => s.Id == id);

            return showtime == null ? null : ToDto(showtime);
        }

        public async Task<(bool Success, string Message)> CreateAsync(CreateShowtimeDto dto)
        {
            var movie = await _context.Movies.FindAsync(dto.MovieId);
            if (movie == null)
                return (false, "Không tìm thấy phim.");

            var hall = await _context.Halls.FindAsync(dto.HallId);
            if (hall == null)
                return (false, "Không tìm thấy phòng chiếu.");

            var endTime = dto.StartTime.AddMinutes(movie.Duration + 15);

            // Kiểm tra xung đột lịch chiếu
            var conflict = await _context.Showtimes.AnyAsync(s =>
                s.HallId == dto.HallId &&
                s.IsActive &&
                s.StartTime < endTime &&
                s.EndTime > dto.StartTime);

            if (conflict)
                return (false, "Phòng chiếu đã có suất chiếu khác trong khung giờ này!");

            var showtime = new Showtime
            {
                MovieId = dto.MovieId,
                HallId = dto.HallId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                Format = Enum.Parse<ShowtimeFormat>(dto.Format),
                BasePrice = dto.BasePrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            // Tạo ShowtimeSeat cho tất cả ghế trong phòng
            var seats = await _context.Seats
                .Where(s => s.HallId == dto.HallId)
                .ToListAsync();

            var showtimeSeats = seats.Select(seat => new ShowtimeSeat
            {
                ShowtimeId = showtime.Id,
                SeatId = seat.Id,
                Status = SeatStatus.Available,
                Price = seat.SeatType == SeatType.VIP
                    ? dto.BasePrice * 1.5m
                    : dto.BasePrice,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.ShowtimeSeats.AddRange(showtimeSeats);
            await _context.SaveChangesAsync();

            return (true, "Tạo suất chiếu thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
                return (false, "Không tìm thấy suất chiếu.");

            showtime.IsActive = false;
            showtime.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, "Xóa suất chiếu thành công!");
        }

        public async Task<IEnumerable<ShowtimeDto>> GetByMovieAsync(int movieId, DateTime? date = null)
        {
            var query = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .Include(s => s.ShowtimeSeats)
                .Where(s => s.MovieId == movieId && s.IsActive && s.StartTime > DateTime.Now);

            if (date.HasValue)
                query = query.Where(s => s.StartTime.Date == date.Value.Date);

            return await query.OrderBy(s => s.StartTime)
                .Select(s => ToDto(s)).ToListAsync();
        }

        private static ShowtimeDto ToDto(Showtime s) => new()
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie.Title,
            MoviePoster = s.Movie.PosterUrl,
            HallId = s.HallId,
            HallName = s.Hall.Name,
            CinemaName = s.Hall.Cinema.Name,
            CinemaId = s.Hall.CinemaId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Format = s.Format.ToString(),
            BasePrice = s.BasePrice,
            IsActive = s.IsActive,
            TotalSeats = s.ShowtimeSeats.Count,
            AvailableSeats = s.ShowtimeSeats.Count(ss => ss.Status == SeatStatus.Available)
        };
    }
}