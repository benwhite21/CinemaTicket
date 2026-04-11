using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Web.Controllers
{
    public class ShowtimeController : Controller
    {
        private readonly IShowtimeService _showtimeService;
        private readonly ApplicationDbContext _context;

        public ShowtimeController(IShowtimeService showtimeService, ApplicationDbContext context)
        {
            _showtimeService = showtimeService;
            _context = context;
        }

        // GET: /Showtime — Danh sách suất chiếu (Admin/Manager)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index(int? movieId, int? cinemaId, DateTime? date)
        {
            var showtimes = await _showtimeService.GetAllAsync(movieId, cinemaId, date);
            ViewBag.Movies = await _context.Movies.ToListAsync();
            ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
            ViewBag.MovieId = movieId;
            ViewBag.CinemaId = cinemaId;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");
            return View(showtimes);
        }

        // GET: /Showtime/Create
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Movies = await _context.Movies
                .Where(m => m.Status != Domain.Enums.MovieStatus.Ended)
                .ToListAsync();
            ViewBag.Halls = await _context.Halls
                .Include(h => h.Cinema)
                .ToListAsync();
            return View();
        }

        // POST: /Showtime/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CreateShowtimeDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _context.Movies.ToListAsync();
                ViewBag.Halls = await _context.Halls.Include(h => h.Cinema).ToListAsync();
                return View(dto);
            }

            var (success, message) = await _showtimeService.CreateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.Movies = await _context.Movies.ToListAsync();
                ViewBag.Halls = await _context.Halls.Include(h => h.Cinema).ToListAsync();
                return View(dto);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Showtime/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _showtimeService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Showtime/ByMovie/5 — Khách chọn suất chiếu
        public async Task<IActionResult> ByMovie(int movieId, DateTime? date)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound();

            if (!date.HasValue) date = DateTime.Today;

            var showtimes = await _showtimeService.GetByMovieAsync(movieId, date);
            var cinemas = showtimes.GroupBy(s => s.CinemaId)
                .Select(g => new { CinemaId = g.Key, CinemaName = g.First().CinemaName, Showtimes = g.ToList() });

            ViewBag.Movie = movie;
            ViewBag.Date = date;
            ViewBag.CinemaGroups = cinemas;
            return View(showtimes);
        }
    }
}