using api.DTOs;

namespace api.Interfaces;

/// <summary>
/// Defines the business logic contract for managing booking requests.
/// </summary>
public interface IBookingRequestService
{
    #region Methods

    /// <summary>
    /// Asynchronously processes and creates a new booking request.
    /// </summary>
    /// <param name="dto">The data transfer object containing the necessary booking details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the booking response details.</returns>
    Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto);

    #endregion
}
