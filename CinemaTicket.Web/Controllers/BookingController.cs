using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Domain.Entities;

namespace CinemaTicket.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(IBookingService bookingService, UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        // GET: /Booking/SelectSeats/5
        public async Task<IActionResult> SelectSeats(int showtimeId)
        {
            var viewModel = await _bookingService.GetSelectSeatsViewModelAsync(showtimeId);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }

        // POST: /Booking/CreateBooking
        [HttpPost]
        public async Task<IActionResult> CreateBooking(int showtimeId, List<int> seatIds)
        {
            if (!seatIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 ghế!";
                return RedirectToAction(nameof(SelectSeats), new { showtimeId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var dto = new CreateBookingDto
            {
                ShowtimeId = showtimeId,
                SeatIds = seatIds
            };

            var (success, message, bookingId) = await _bookingService.CreateBookingAsync(dto, user.Id);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(SelectSeats), new { showtimeId });
            }

            return RedirectToAction(nameof(Confirm), new { id = bookingId });
        }

        // GET: /Booking/Confirm/5
        public async Task<IActionResult> Confirm(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null) return NotFound();

            // Giả lập thanh toán thành công (sẽ tích hợp VNPay sau)
            return View(booking);
        }

        // POST: /Booking/FakePayment
        [HttpPost]
        public async Task<IActionResult> FakePayment(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null) return NotFound();

            TempData["Success"] = "Thanh toán thành công! Vé đã được xác nhận.";
            return RedirectToAction(nameof(MyBookings));
        }

        // GET: /Booking/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
            return View(bookings);
        }

        // POST: /Booking/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (success, message) = await _bookingService.CancelBookingAsync(id, user.Id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(MyBookings));
        }
    }
}