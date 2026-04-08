using CinemaTicket.Domain.Enums;

namespace CinemaTicket.Domain.Entities
{
    public class Movie : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? OriginalTitle { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string Rating { get; set; } = "P";
        public MovieStatus Status { get; set; } = MovieStatus.ComingSoon;

        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}