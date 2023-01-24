using DAL.Restaurant.Kithen.Entities;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Booking;

public class RestaurantBookingDbContext : DbContext
{
    public RestaurantBookingDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<ProcessedMessage> ProcessedMessages { get; set; } = null;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}