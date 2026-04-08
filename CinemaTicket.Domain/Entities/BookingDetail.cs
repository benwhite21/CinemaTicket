namespace CinemaTicket.Domain.Entities
{
    public class BookingDetail : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;

        public decimal Price { get; set; }
        public string SeatName { get; set; } = string.Empty;
    }
}