using Microsoft.AspNetCore.Identity;
using CinemaTicket.Domain.Entities;

namespace CinemaTicket.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}