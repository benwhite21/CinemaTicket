using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Domain.Entities;
using QRCoder;

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
        // Thêm action này vào BookingController
        public IActionResult QRCode(string code)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(10);
            return File(qrBytes, "image/png");
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
            var (success, message) = await _bookingService.ConfirmPaymentAsync(bookingId);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Confirm), new { id = bookingId });
            }

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