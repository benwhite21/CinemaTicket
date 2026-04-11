namespace CinemaTicket.Application.DTOs
{
    public class CinemaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
        public int TotalHalls { get; set; }
    }

    public class CreateCinemaDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
    }

    public class HallDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int CinemaId { get; set; }
        public string CinemaName { get; set; } = string.Empty;
    }

    public class CreateHallDto
    {
        public string Name { get; set; } = string.Empty;
        public int CinemaId { get; set; }
        public int Rows { get; set; } = 8;
        public int Columns { get; set; } = 10;
        public int VipRowsFromBack { get; set; } = 2;
    }

    public class SeatDto
    {
        public int Id { get; set; }
        public string Row { get; set; } = string.Empty;
        public int Column { get; set; }
        public string SeatType { get; set; } = string.Empty;
        public string SeatName => $"{Row}{Column}";
    }
}