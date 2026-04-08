namespace CinemaTicket.Domain.Entities
{
    public class Hall : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}