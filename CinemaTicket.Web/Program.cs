using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using CinemaTicket.Domain.Enums;
using CinemaTicket.Infrastructure.Data;
using CinemaTicket.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ICinemaService, CinemaService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    context.Database.Migrate();

    // Seed roles
    string[] roles = { "Admin", "Manager", "Staff", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed admin account
    var adminEmail = "admin@cinema.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(admin, "Admin@123456");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed genres
    if (!context.Genres.Any())
    {
        var genres = new List<Genre>
        {
            new Genre { Name = "Hành động", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Tình cảm", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Hoạt hình", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Kinh dị", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Hài", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Phiêu lưu", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Khoa học viễn tưởng", CreatedAt = DateTime.UtcNow },
        };
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();
    }

    // Seed movies
    if (!context.Movies.Any())
    {
        var actionGenre = context.Genres.First(g => g.Name == "Hành động");
        var horrorGenre = context.Genres.First(g => g.Name == "Kinh dị");
        var animationGenre = context.Genres.First(g => g.Name == "Hoạt hình");
        var adventureGenre = context.Genres.First(g => g.Name == "Phiêu lưu");

        var movies = new List<Movie>
        {
            new Movie
            {
                Title = "Vũ Trụ Tối Thượng",
                OriginalTitle = "Ultimate Universe",
                Description = "Một siêu anh hùng đối mặt với kẻ thù mạnh nhất vũ trụ.",
                Duration = 148,
                ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                Director = "Nguyễn Văn Đạo",
                Cast = "Trần Minh, Lê Hoa, Phạm Tuấn",
                Rating = "C13",
                Status = MovieStatus.NowShowing,
                CreatedAt = DateTime.UtcNow,
                MovieGenres = new List<MovieGenre>
                {
                    new MovieGenre { GenreId = actionGenre.Id }
                }
            },
            new Movie
            {
                Title = "Đại Dương Bí Ẩn",
                OriginalTitle = "Mystery Ocean",
                Description = "Cuộc phiêu lưu dưới đáy đại dương đầy bí ẩn.",
                Duration = 132,
                ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                Director = "Lê Thị Hương",
                Cast = "Phạm An, Trần Bình",
                Rating = "P",
                Status = MovieStatus.NowShowing,
                CreatedAt = DateTime.UtcNow,
                MovieGenres = new List<MovieGenre>
                {
                    new MovieGenre { GenreId = adventureGenre.Id }
                }
            },
            new Movie
            {
                Title = "Đêm Không Ngủ",
                OriginalTitle = "Sleepless Night",
                Description = "Một đêm kinh hoàng không thể thoát khỏi.",
                Duration = 115,
                ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                Director = "Hoàng Minh",
                Cast = "Nguyễn Lan, Trần Đức",
                Rating = "C18",
                Status = MovieStatus.NowShowing,
                CreatedAt = DateTime.UtcNow,
                MovieGenres = new List<MovieGenre>
                {
                    new MovieGenre { GenreId = horrorGenre.Id }
                }
            },
            new Movie
            {
                Title = "Khu Rừng Xanh",
                OriginalTitle = "Green Forest",
                Description = "Câu chuyện cảm động về tình bạn giữa người và động vật.",
                Duration = 98,
                ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Director = "Phạm Thu",
                Cast = "Lê Nam, Nguyễn Hà",
                Rating = "P",
                Status = MovieStatus.ComingSoon,
                CreatedAt = DateTime.UtcNow,
                MovieGenres = new List<MovieGenre>
                {
                    new MovieGenre { GenreId = animationGenre.Id }
                }
            },
        };
        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();
    }

    // Seed cinemas
    if (!context.Cinemas.Any())
    {
        var cinemas = new List<Cinema>
        {
            new Cinema
            {
                Name = "CGV Vincom",
                Address = "191 Bà Triệu, Hai Bà Trưng, Hà Nội",
                Phone = "024 3974 3333",
                City = "Hà Nội",
                CreatedAt = DateTime.UtcNow
            },
            new Cinema
            {
                Name = "Lotte Cinema",
                Address = "54 Liễu Giai, Ba Đình, Hà Nội",
                Phone = "024 3333 0909",
                City = "Hà Nội",
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Cinemas.AddRange(cinemas);
        await context.SaveChangesAsync();
    }
    // Seed halls và seats
    if (!context.Halls.Any())
    {
        var cgv = context.Cinemas.First(c => c.Name == "CGV Vincom");
        var lotte = context.Cinemas.First(c => c.Name == "Lotte Cinema");

        var halls = new List<Hall>
    {
        new Hall { Name = "Phòng 1 - Standard", CinemaId = cgv.Id, TotalSeats = 80, CreatedAt = DateTime.UtcNow },
        new Hall { Name = "Phòng 2 - IMAX", CinemaId = cgv.Id, TotalSeats = 60, CreatedAt = DateTime.UtcNow },
        new Hall { Name = "Phòng 1 - Standard", CinemaId = lotte.Id, TotalSeats = 80, CreatedAt = DateTime.UtcNow },
    };
        context.Halls.AddRange(halls);
        await context.SaveChangesAsync();

        // Tạo ghế cho từng phòng
        foreach (var hall in context.Halls.ToList())
        {
            int rows = 8, cols = 10;
            var seats = new List<Seat>();
            for (int r = 0; r < rows; r++)
            {
                string rowLetter = ((char)('A' + r)).ToString();
                for (int c = 1; c <= cols; c++)
                {
                    seats.Add(new Seat
                    {
                        HallId = hall.Id,
                        Row = rowLetter,
                        Column = c,
                        SeatType = r >= rows - 2 ? SeatType.VIP : SeatType.Standard,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.Seats.AddRange(seats);
        }
        await context.SaveChangesAsync();
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();