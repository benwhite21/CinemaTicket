using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SelectSeatsViewModel?> GetSelectSeatsViewModelAsync(int showtimeId)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null) return null;

            var showtimeSeats = await _context.ShowtimeSeats
                .Include(ss => ss.Seat)
                .Where(ss => ss.ShowtimeId == showtimeId)
                .OrderBy(ss => ss.Seat.Row)
                .ThenBy(ss => ss.Seat.Column)
                .ToListAsync();

            return new SelectSeatsViewModel
            {
                Showtime = new ShowtimeDto
                {
                    Id = showtime.Id,
                    MovieId = showtime.MovieId,
                    MovieTitle = showtime.Movie.Title,
                    MoviePoster = showtime.Movie.PosterUrl,
                    HallId = showtime.HallId,
                    HallName = showtime.Hall.Name,
                    CinemaName = showtime.Hall.Cinema.Name,
                    CinemaId = showtime.Hall.CinemaId,
                    StartTime = showtime.StartTime,
                    EndTime = showtime.EndTime,
                    Format = showtime.Format.ToString(),
                    BasePrice = showtime.BasePrice,
                    IsActive = showtime.IsActive,
                    TotalSeats = showtimeSeats.Count,
                    AvailableSeats = showtimeSeats.Count(ss => ss.Status == SeatStatus.Available)
                },
                Seats = showtimeSeats.Select(ss => new SeatWithStatusDto
                {
                    Id = ss.SeatId,
                    Row = ss.Seat.Row,
                    Column = ss.Seat.Column,
                    SeatType = ss.Seat.SeatType.ToString(),
                    Status = ss.Status.ToString(),
                    Price = ss.Price
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(
            CreateBookingDto dto, string userId)
        {
            using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var showtimeSeats = await _context.ShowtimeSeats
                    .Where(ss => ss.ShowtimeId == dto.ShowtimeId
                        && dto.SeatIds.Contains(ss.SeatId))
                    .ToListAsync();

                if (showtimeSeats.Count != dto.SeatIds.Count)
                    return (false, "Một số ghế không tồn tại.", null);

                var unavailable = showtimeSeats
                    .Where(ss => ss.Status != SeatStatus.Available)
                    .ToList();

                if (unavailable.Any())
                    return (false, "Một số ghế đã được đặt. Vui lòng chọn ghế khác.", null);

                foreach (var ss in showtimeSeats)
                {
                    ss.Status = SeatStatus.Locked;
                    ss.LockedAt = DateTime.UtcNow;
                    ss.LockedByUserId = userId;
                }

                var totalAmount = showtimeSeats.Sum(ss => ss.Price);
                var bookingCode = $"CB-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

                var booking = new Booking
                {
                    UserId = userId,
                    ShowtimeId = dto.ShowtimeId,
                    BookingCode = bookingCode,
                    TotalAmount = totalAmount,
                    DiscountAmount = 0,
                    FinalAmount = totalAmount,
                    Status = BookingStatus.Pending,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(8),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var details = new List<BookingDetail>();
                foreach (var ss in showtimeSeats)
                {
                    var seat = await _context.Seats.FindAsync(ss.SeatId);
                    details.Add(new BookingDetail
                    {
                        BookingId = booking.Id,
                        SeatId = ss.SeatId,
                        Price = ss.Price,
                        SeatName = $"{seat!.Row}{seat.Column}",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.BookingDetails.AddRange(details);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Đặt vé thành công!", booking.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}", null);
            }
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Hall).ThenInclude(h => h.Cinema)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return null;
            return ToDto(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Hall).ThenInclude(h => h.Cinema)
                .Include(b => b.BookingDetails)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => ToDto(b))
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CancelBookingAsync(int id, string userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
                return (false, "Không tìm thấy đơn đặt vé.");

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Paid)
                return (false, "Không thể hủy đơn này.");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            var seatIds = booking.BookingDetails.Select(d => d.SeatId).ToList();
            var showtimeSeats = await _context.ShowtimeSeats
                .Where(ss => ss.ShowtimeId == booking.ShowtimeId
                    && seatIds.Contains(ss.SeatId))
                .ToListAsync();

            foreach (var ss in showtimeSeats)
            {
                ss.Status = SeatStatus.Available;
                ss.LockedAt = null;
                ss.LockedByUserId = null;
            }

            await _context.SaveChangesAsync();
            return (true, "Hủy đặt vé thành công!");
        }

        public async Task ExpireOldBookingsAsync()
        {
            var expiredBookings = await _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.Status == BookingStatus.Pending
                    && b.ExpiredAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.Expired;
                booking.UpdatedAt = DateTime.UtcNow;

                var seatIds = booking.BookingDetails.Select(d => d.SeatId).ToList();
                var showtimeSeats = await _context.ShowtimeSeats
                    .Where(ss => ss.ShowtimeId == booking.ShowtimeId
                        && seatIds.Contains(ss.SeatId))
                    .ToListAsync();

                foreach (var ss in showtimeSeats)
                {
                    ss.Status = SeatStatus.Available;
                    ss.LockedAt = null;
                    ss.LockedByUserId = null;
                }
            }

            await _context.SaveChangesAsync();
        }

        private static BookingDto ToDto(Booking b) => new()
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            UserFullName = b.User?.FullName ?? "",
            UserEmail = b.User?.Email ?? "",
            MovieTitle = b.Showtime?.Movie?.Title ?? "",
            HallName = b.Showtime?.Hall?.Name ?? "",
            CinemaName = b.Showtime?.Hall?.Cinema?.Name ?? "",
            StartTime = b.Showtime?.StartTime ?? DateTime.MinValue,
            TotalAmount = b.TotalAmount,
            DiscountAmount = b.DiscountAmount,
            FinalAmount = b.FinalAmount,
            Status = b.Status.ToString(),
            BookedAt = b.CreatedAt,
            ExpiredAt = b.ExpiredAt,
            Details = b.BookingDetails?.Select(d => new BookingDetailDto
            {
                SeatName = d.SeatName,
                SeatType = "",
                Price = d.Price
            }).ToList() ?? new()
        };
    }
}