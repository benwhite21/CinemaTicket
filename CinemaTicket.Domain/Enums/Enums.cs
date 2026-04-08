namespace CinemaTicket.Domain.Enums
{
    public enum MovieStatus
    {
        Hidden = 0,
        ComingSoon = 1,
        NowShowing = 2,
        Ended = 3
    }

    public enum SeatType
    {
        Standard = 0,
        VIP = 1,
        Couple = 2
    }

    public enum SeatStatus
    {
        Available = 0,
        Locked = 1,
        Booked = 2
    }

    public enum BookingStatus
    {
        Pending = 1,
        Paid = 2,
        Expired = 3,
        Cancelled = 4,
        Used = 5
    }

    public enum ShowtimeFormat
    {
        Standard2D = 0,
        ThreeD = 1,
        IMAX = 2
    }

    public enum PaymentMethod
    {
        VNPay = 0,
        Momo = 1,
        Cash = 2,
        CreditCard = 3
    }
}