using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Infrastructure.Services
{
    public class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MovieService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<MovieDto>> GetAllAsync(string? search = null, int? genreId = null, string? status = null)
        {
            var query = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<MovieStatus>(status, out var movieStatus))
                query = query.Where(m => m.Status == movieStatus);

            return await query.Select(m => ToDto(m)).ToListAsync();
        }

        public async Task<MovieDto?> GetByIdAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            return movie == null ? null : ToDto(movie);
        }

        public async Task<(bool Success, string Message)> CreateAsync(CreateMovieDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                OriginalTitle = dto.OriginalTitle,
                Description = dto.Description,
                Duration = dto.Duration,
                ReleaseDate = dto.ReleaseDate,
                EndDate = dto.EndDate,
                Director = dto.Director,
                Cast = dto.Cast,
                TrailerUrl = dto.TrailerUrl,
                Rating = dto.Rating,
                Status = MovieStatus.ComingSoon,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Poster != null)
                movie.PosterUrl = await SavePosterAsync(dto.Poster);

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            foreach (var genreId in dto.GenreIds)
                _context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genreId });

            await _context.SaveChangesAsync();
            return (true, "Thêm phim thành công!");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(EditMovieDto dto)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Id == dto.Id);

            if (movie == null)
                return (false, "Không tìm thấy phim.");

            movie.Title = dto.Title;
            movie.OriginalTitle = dto.OriginalTitle;
            movie.Description = dto.Description;
            movie.Duration = dto.Duration;
            movie.ReleaseDate = dto.ReleaseDate;
            movie.EndDate = dto.EndDate;
            movie.Director = dto.Director;
            movie.Cast = dto.Cast;
            movie.TrailerUrl = dto.TrailerUrl;
            movie.Rating = dto.Rating;
            movie.UpdatedAt = DateTime.UtcNow;

            if (dto.Poster != null)
                movie.PosterUrl = await SavePosterAsync(dto.Poster);

            _context.MovieGenres.RemoveRange(movie.MovieGenres);
            foreach (var genreId in dto.GenreIds)
                _context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genreId });

            await _context.SaveChangesAsync();
            return (true, "Cập nhật phim thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return (false, "Không tìm thấy phim.");

            movie.IsDeleted = true;
            movie.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, "Xóa phim thành công!");
        }

        public async Task<IEnumerable<MovieDto>> GetNowShowingAsync() =>
            await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Where(m => m.Status == MovieStatus.NowShowing)
                .Select(m => ToDto(m)).ToListAsync();

        public async Task<IEnumerable<MovieDto>> GetComingSoonAsync() =>
            await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Where(m => m.Status == MovieStatus.ComingSoon)
                .Select(m => ToDto(m)).ToListAsync();

        private async Task<string> SavePosterAsync(IFormFile poster)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "posters");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(poster.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await poster.CopyToAsync(stream);
            return $"/images/posters/{fileName}";
        }

        private static MovieDto ToDto(Movie m) => new()
        {
            Id = m.Id,
            Title = m.Title,
            OriginalTitle = m.OriginalTitle,
            Description = m.Description,
            Duration = m.Duration,
            ReleaseDate = m.ReleaseDate,
            EndDate = m.EndDate,
            Director = m.Director,
            Cast = m.Cast,
            PosterUrl = m.PosterUrl,
            TrailerUrl = m.TrailerUrl,
            Rating = m.Rating,
            Status = m.Status.ToString(),
            Genres = m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
        };
    }
}