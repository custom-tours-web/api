using api.Datas;
using api.Interfaces;
using api.Models;

namespace api.Repositories;

/// <summary>
/// Implements data access operations for booking requests.
/// <paramref name="context"/> is the database context for interacting with the data store.
/// </summary>
public class BookingRequestRepository(TourismDbContext context) : IBookingRequestRepository
{
    #region Dependencies

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
        await _context.BookingRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    #endregion
}
