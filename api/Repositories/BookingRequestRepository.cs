using api.Datas;
using api.Interfaces;
using api.Models;

namespace api.Repositories;

/// <summary>
/// Implements data access operations for booking requests.
/// <paramref name="logger"/> is used for logging data access operations and errors.
/// <paramref name="context"/> is the database context for interacting with the data store.
/// </summary>
public class BookingRequestRepository(
    ILogger<BookingRequestRepository> logger,
    TourismDbContext context) : IBookingRequestRepository
{
    #region Dependencies

    /// <summary>
    /// The logger instance for logging data access operations and errors within the repository layer.
    /// </summary>
    private readonly ILogger<BookingRequestRepository> _logger = logger;

    /// <summary>
    /// The database context for interacting with the underlying data store.
    /// </summary>
    private readonly TourismDbContext _context = context;

    #endregion

    #region Data Access

    /// <summary>
    /// Asynchronously adds a new booking request to the database and commits the transaction.
    /// </summary>
    /// <param name="request">The booking request entity to insert.</param>
    /// <returns>A task that represents the asynchronous database operation.</returns>
    public async Task AddAsync(BookingRequest request)
    {
        try
        {
            _logger.LogDebug("Attempting to add a new booking request to the database context.");

            await _context.BookingRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Successfully saved the booking request to the database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A data access error occurred while attempting to save a new booking request to the database.");
            throw;
        }
    }

    #endregion
}
