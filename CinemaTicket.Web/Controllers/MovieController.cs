using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ApplicationDbContext _context;

        public MovieController(IMovieService movieService, ApplicationDbContext context)
        {
            _movieService = movieService;
            _context = context;
        }

        // GET: /Movie — Trang danh sách phim public
        public async Task<IActionResult> Index(string? search, int? genreId, string? status)
        {
            var movies = await _movieService.GetAllAsync(search, genreId, status);
            var genres = await _context.Genres.ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.Search = search;
            ViewBag.GenreId = genreId;
            ViewBag.Status = status;

            return View(movies);
        }

        // GET: /Movie/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieService.GetByIdAsync(id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // GET: /Movie/Create — Chỉ Admin/Manager
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = await _context.Genres.ToListAsync();
            return View();
        }

        // POST: /Movie/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CreateMovieDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = await _context.Genres.ToListAsync();
                return View(dto);
            }

            var (success, message) = await _movieService.CreateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.Genres = await _context.Genres.ToListAsync();
                return View(dto);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Movie/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieService.GetByIdAsync(id);
            if (movie == null)
                return NotFound();

            var dto = new EditMovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                OriginalTitle = movie.OriginalTitle,
                Description = movie.Description,
                Duration = movie.Duration,
                ReleaseDate = movie.ReleaseDate,
                EndDate = movie.EndDate,
                Director = movie.Director,
                Cast = movie.Cast,
                TrailerUrl = movie.TrailerUrl,
                Rating = movie.Rating,
                ExistingPosterUrl = movie.PosterUrl
            };

            ViewBag.Genres = await _context.Genres.ToListAsync();
            return View(dto);
        }

        // POST: /Movie/Edit
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(EditMovieDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = await _context.Genres.ToListAsync();
                return View(dto);
            }

            var (success, message) = await _movieService.UpdateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.Genres = await _context.Genres.ToListAsync();
                return View(dto);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Movie/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _movieService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}