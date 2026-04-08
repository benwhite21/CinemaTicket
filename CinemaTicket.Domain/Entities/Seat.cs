using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class Seat : BaseEntity
    {
        public string Row { get; set; } = string.Empty;
        public int Column { get; set; }
        public SeatType SeatType { get; set; } = SeatType.Standard;
        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;
    }
}