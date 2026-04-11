using Microsoft.AspNetCore.Http;
namespace CinemaTicket.Application.DTOs

{
    public class MovieDto
    {
        public int Id { get; set; }
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
        public string Status { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
    }

    public class CreateMovieDto
    {
        public string Title { get; set; } = string.Empty;
        public string? OriginalTitle { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? TrailerUrl { get; set; }
        public string Rating { get; set; } = "P";
        public List<int> GenreIds { get; set; } = new();
        public IFormFile? Poster { get; set; }
    }

    public class EditMovieDto : CreateMovieDto
    {
        public int Id { get; set; }
        public string? ExistingPosterUrl { get; set; }
    }
}