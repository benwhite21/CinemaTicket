using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Infrastructure.Services
{
    public class CinemaService : ICinemaService
    {
        private readonly ApplicationDbContext _context;

        public CinemaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CinemaDto>> GetAllCinemasAsync()
        {
            return await _context.Cinemas
                .Include(c => c.Halls)
                .Select(c => new CinemaDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    Phone = c.Phone,
                    City = c.City,
                    TotalHalls = c.Halls.Count
                }).ToListAsync();
        }

        public async Task<CinemaDto?> GetCinemaByIdAsync(int id)
        {
            var cinema = await _context.Cinemas
                .Include(c => c.Halls)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null) return null;

            return new CinemaDto
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Address = cinema.Address,
                Phone = cinema.Phone,
                City = cinema.City,
                TotalHalls = cinema.Halls.Count
            };
        }

        public async Task<(bool Success, string Message)> CreateCinemaAsync(CreateCinemaDto dto)
        {
            var cinema = new Cinema
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                City = dto.City,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();
            return (true, "Thêm rạp thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteCinemaAsync(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
                return (false, "Không tìm thấy rạp.");

            cinema.IsDeleted = true;
            await _context.SaveChangesAsync();
            return (true, "Xóa rạp thành công!");
        }

        public async Task<IEnumerable<HallDto>> GetHallsByCinemaAsync(int cinemaId)
        {
            return await _context.Halls
                .Where(h => h.CinemaId == cinemaId)
                .Select(h => new HallDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    TotalSeats = h.TotalSeats,
                    CinemaId = h.CinemaId,
                    CinemaName = h.Cinema.Name
                }).ToListAsync();
        }

        public async Task<HallDto?> GetHallByIdAsync(int id)
        {
            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hall == null) return null;

            return new HallDto
            {
                Id = hall.Id,
                Name = hall.Name,
                TotalSeats = hall.TotalSeats,
                CinemaId = hall.CinemaId,
                CinemaName = hall.Cinema.Name
            };
        }

        public async Task<(bool Success, string Message)> CreateHallAsync(CreateHallDto dto)
        {
            var totalSeats = dto.Rows * dto.Columns;

            var hall = new Hall
            {
                Name = dto.Name,
                CinemaId = dto.CinemaId,
                TotalSeats = totalSeats,
                CreatedAt = DateTime.UtcNow
            };

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            // Tự động tạo ghế
            var seats = new List<Seat>();
            for (int r = 0; r < dto.Rows; r++)
            {
                string rowLetter = ((char)('A' + r)).ToString();
                for (int c = 1; c <= dto.Columns; c++)
                {
                    // Hàng cuối là VIP
                    var seatType = r >= dto.Rows - dto.VipRowsFromBack
                        ? SeatType.VIP
                        : SeatType.Standard;

                    seats.Add(new Seat
                    {
                        HallId = hall.Id,
                        Row = rowLetter,
                        Column = c,
                        SeatType = seatType,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();
            return (true, $"Tạo phòng chiếu thành công với {totalSeats} ghế!");
        }

        public async Task<IEnumerable<SeatDto>> GetSeatsByHallAsync(int hallId)
        {
            return await _context.Seats
                .Where(s => s.HallId == hallId)
                .OrderBy(s => s.Row).ThenBy(s => s.Column)
                .Select(s => new SeatDto
                {
                    Id = s.Id,
                    Row = s.Row,
                    Column = s.Column,
                    SeatType = s.SeatType.ToString()
                }).ToListAsync();
        }
    }
}