using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public bool IsSuccess { get; set; } = false;
        public DateTime? PaidAt { get; set; }
        public string? Note { get; set; }
    }
}