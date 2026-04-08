using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class Showtime : BaseEntity
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ShowtimeFormat Format { get; set; } = ShowtimeFormat.Standard2D;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<ShowtimeSeat> ShowtimeSeats { get; set; } = new List<ShowtimeSeat>();
    }
}