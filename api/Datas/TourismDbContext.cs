using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace api.Datas;

/// <summary>
/// Represents the primary database context for the Tourism application.
/// Manages entity configurations, database connections, and data persistence.
/// </summary>
public partial class TourismDbContext : DbContext
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TourismDbContext"/> class.
    /// Primarily used for design-time operations (e.g., EF Core migrations).
    /// </summary>
    public TourismDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TourismDbContext"/> class with the specified options.
    /// Used by the Dependency Injection container at runtime.
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
        // Add specific entity configurations here or apply configurations from an assembly
        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// A partial method hook for external configuration injected by source generators or partial classes.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    #endregion

    #region Save Changes (Logging & Exception Handling)

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database, with robust exception logging.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogDatabaseError(ex, "A concurrency error occurred while saving entities to the database.");
            throw;
        }
        catch (DbUpdateException ex)
        {
            LogDatabaseError(ex, "A database update error occurred. Please check entity constraints or data integrity.");
            throw;
        }
        catch (Exception ex)
        {
            LogDatabaseError(ex, "An unexpected error occurred while saving database changes.");
            throw;
        }
    }

    /// <summary>
    /// Helper method to retrieve the logger from EF Core's internal service provider and log the exception.
    /// </summary>
    private void LogDatabaseError(Exception ex, string message)
    {
        var logger = this.GetService<ILogger<TourismDbContext>>();
        logger?.LogError(ex, "{Message}", message);
    }

    #endregion
}
