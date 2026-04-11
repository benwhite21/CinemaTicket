using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CinemaController : Controller
    {
        private readonly ICinemaService _cinemaService;

        public CinemaController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService;
        }

        // GET: /Cinema
        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();
            return View(cinemas);
        }

        // GET: /Cinema/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Cinema/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateCinemaDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, message) = await _cinemaService.CreateCinemaAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(dto);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cinema/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _cinemaService.DeleteCinemaAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cinema/Halls/5
        public async Task<IActionResult> Halls(int id)
        {
            var cinema = await _cinemaService.GetCinemaByIdAsync(id);
            if (cinema == null) return NotFound();

            var halls = await _cinemaService.GetHallsByCinemaAsync(id);
            ViewBag.Cinema = cinema;
            return View(halls);
        }

        // GET: /Cinema/CreateHall/5
        public async Task<IActionResult> CreateHall(int cinemaId)
        {
            var cinema = await _cinemaService.GetCinemaByIdAsync(cinemaId);
            if (cinema == null) return NotFound();

            ViewBag.Cinema = cinema;
            return View(new CreateHallDto { CinemaId = cinemaId });
        }

        // POST: /Cinema/CreateHall
        [HttpPost]
        public async Task<IActionResult> CreateHall(CreateHallDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cinema = await _cinemaService.GetCinemaByIdAsync(dto.CinemaId);
                return View(dto);
            }

            var (success, message) = await _cinemaService.CreateHallAsync(dto);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Halls), new { id = dto.CinemaId });
        }

        // GET: /Cinema/SeatMap/5
        public async Task<IActionResult> SeatMap(int hallId)
        {
            var hall = await _cinemaService.GetHallByIdAsync(hallId);
            if (hall == null) return NotFound();

            var seats = await _cinemaService.GetSeatsByHallAsync(hallId);
            ViewBag.Hall = hall;
            return View(seats);
        }
    }
}