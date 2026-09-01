using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Datas;

public partial class TourismDbContext : DbContext
{
    public TourismDbContext()
    {
    }

    public TourismDbContext(DbContextOptions<TourismDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookingRequest> BookingRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
