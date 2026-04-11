namespace CinemaTicket.Application.DTOs
{
    public class ShowtimeDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePoster { get; set; }
        public int HallId { get; set; }
        public string HallName { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public int CinemaId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Format { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
    }

    public class CreateShowtimeDto
    {
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public string Format { get; set; } = "Standard2D";
        public decimal BasePrice { get; set; }
    }
}