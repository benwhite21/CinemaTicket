using CinemaTicket.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<ShowtimeSeat> ShowtimeSeats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Khóa chính kép cho MovieGenre
            builder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            // Booking code là unique
            builder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();

            // Soft delete filter
            builder.Entity<Movie>()
                .HasQueryFilter(m => !m.IsDeleted);
            builder.Entity<Cinema>()
                .HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Hall>()
                .HasQueryFilter(h => !h.IsDeleted);

            // Cấu hình decimal precision
            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Booking>()
                .Property(b => b.DiscountAmount)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Booking>()
                .Property(b => b.FinalAmount)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Showtime>()
                .Property(s => s.BasePrice)
                .HasColumnType("decimal(18,2)");
            builder.Entity<ShowtimeSeat>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
            builder.Entity<BookingDetail>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)");

            // Fix cascade paths cho ShowtimeSeats
            builder.Entity<ShowtimeSeat>()
                .HasOne(ss => ss.Showtime)
                .WithMany(s => s.ShowtimeSeats)
                .HasForeignKey(ss => ss.ShowtimeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ShowtimeSeat>()
                .HasOne(ss => ss.Seat)
                .WithMany()
                .HasForeignKey(ss => ss.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix cascade paths cho BookingDetail
            builder.Entity<BookingDetail>()
                .HasOne(bd => bd.Booking)
                .WithMany(b => b.BookingDetails)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}