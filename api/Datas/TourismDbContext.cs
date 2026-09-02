using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Datas;

/// <summary>
/// Represents the primary database context for the Tourism application.
/// </summary>
public partial class TourismDbContext : DbContext
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TourismDbContext"/> class.
    /// </summary>
    public TourismDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TourismDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The configuration options for this context.</param>
    public TourismDbContext(DbContextOptions<TourismDbContext> options)
        : base(options)
    {
    }

    #endregion

    #region DbSets

    /// <summary>
    /// Gets or sets the collection of booking requests in the database.
    /// </summary>
    public virtual DbSet<BookingRequest> BookingRequests { get; set; }

    #endregion

    #region Configuration

    /// <summary>
    /// Configures the database connections and options.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Configures the database schema, entity models, and relationships.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// A partial method hook for external configuration injected by source generators or partial classes.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    #endregion
}
