using System;

namespace CinemaTicket.Domain.Entities
{
    public class Cinema : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }

        public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    }
}