using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class ShowtimeSeat : BaseEntity
    {
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;

        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;

        public SeatStatus Status { get; set; } = SeatStatus.Available;
        public decimal Price { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedByUserId { get; set; }
    }
}