using CinemaTicket.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nowShowing = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Where(m => m.Status == MovieStatus.NowShowing)
                .Take(8).ToListAsync();

            var comingSoon = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Where(m => m.Status == MovieStatus.ComingSoon)
                .Take(4).ToListAsync();

            var totalCinemas = await _context.Cinemas.CountAsync();
            var totalMovies = await _context.Movies.CountAsync();
            var totalBookings = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid).CountAsync();

            ViewBag.NowShowing = nowShowing;
            ViewBag.ComingSoon = comingSoon;
            ViewBag.TotalCinemas = totalCinemas;
            ViewBag.TotalMovies = totalMovies;
            ViewBag.TotalBookings = totalBookings;

            return View();
        }

        public IActionResult Privacy() => View();
    }
}