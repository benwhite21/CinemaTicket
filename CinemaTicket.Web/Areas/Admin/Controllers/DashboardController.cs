using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Tổng doanh thu hôm nay
            var revenueToday = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid
                    && b.UpdatedAt.HasValue
                    && b.UpdatedAt.Value.Date == today)
                .SumAsync(b => b.FinalAmount);

            // Vé bán hôm nay
            var ticketsSoldToday = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid
                    && b.UpdatedAt.HasValue
                    && b.UpdatedAt.Value.Date == today)
                .CountAsync();

            // Suất chiếu hôm nay
            var showtimesToday = await _context.Showtimes
                .Where(s => s.StartTime.Date == today && s.IsActive)
                .CountAsync();

            // Tổng phim đang chiếu
            var nowShowingCount = await _context.Movies
                .Where(m => m.Status == MovieStatus.NowShowing)
                .CountAsync();

            // Doanh thu 7 ngày
            var revenue7Days = new List<object>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var rev = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Paid
                        && b.UpdatedAt.HasValue
                        && b.UpdatedAt.Value.Date == date)
                    .SumAsync(b => b.FinalAmount);

                revenue7Days.Add(new { date = date.ToString("dd/MM"), revenue = rev });
            }

            // Top phim
            var topMovies = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid)
                .GroupBy(b => b.Showtime.Movie.Title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Vé vừa đặt
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.CreatedAt)
                .Take(7)
                .ToListAsync();

            // Tổng users
            var totalUsers = await _context.Users.CountAsync();

            // Tổng rạp
            var totalCinemas = await _context.Cinemas.CountAsync();

            ViewBag.RevenueToday = revenueToday;
            ViewBag.TicketsSoldToday = ticketsSoldToday;
            ViewBag.ShowtimesToday = showtimesToday;
            ViewBag.NowShowingCount = nowShowingCount;
            ViewBag.Revenue7Days = revenue7Days;
            ViewBag.TopMovies = topMovies;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCinemas = totalCinemas;

            return View();
        }
    }
}