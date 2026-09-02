using api.Models;

namespace api.Interfaces;

/// <summary>
/// Defines the contract for data access operations related to booking requests.
/// </summary>
public interface IBookingRequestRepository
{
    #region Methods

    /// <summary>
    /// Asynchronously adds a new booking request to the underlying data store.
    /// </summary>
    /// <param name="request">The booking request entity to be persisted.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddAsync(BookingRequest request);

    #endregion
}
