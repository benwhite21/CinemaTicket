namespace CinemaTicket.Application.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public List<BookingDetailDto> Details { get; set; } = new();
    }

    public class BookingDetailDto
    {
        public string SeatName { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class CreateBookingDto
    {
        public int ShowtimeId { get; set; }
        public List<int> SeatIds { get; set; } = new();
    }

    public class SelectSeatsViewModel
    {
        public ShowtimeDto Showtime { get; set; } = null!;
        public List<SeatWithStatusDto> Seats { get; set; } = new();
    }

    public class SeatWithStatusDto
    {
        public int Id { get; set; }
        public string Row { get; set; } = string.Empty;
        public int Column { get; set; }
        public string SeatType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SeatName => $"{Row}{Column}";
    }
}