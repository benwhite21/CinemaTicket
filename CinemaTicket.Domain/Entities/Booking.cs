using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;

        public string BookingCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime? ExpiredAt { get; set; }

        public Payment? Payment { get; set; }
        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}