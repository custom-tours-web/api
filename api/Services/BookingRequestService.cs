using api.DTOs;
using api.Interfaces;
using api.Models;
using AutoMapper;

namespace api.Services;

/// <summary>
/// Orchestrates data mapping, persistence via the repository, and returning standardized responses.
/// </summary>
public class BookingRequestService(
    IMapper mapper,
    IBookingRequestRepository repository,
    ILogger<BookingRequestService> logger) : IBookingRequestService
{
    #region Dependencies

    /// <summary>
    /// The AutoMapper instance for mapping between DTOs and domain entities.
    /// </summary>
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// The logger instance for logging service-level operations and errors.
    /// </summary>
    private readonly ILogger<BookingRequestService> _logger = logger;

    /// <summary>
    /// The repository instance for performing data access operations related to booking requests.
    /// </summary>
    private readonly IBookingRequestRepository _repository = repository;

    #endregion

    #region Business Logic

    /// <summary>
    /// Asynchronously processes a booking request by mapping the DTO to an entity and persisting it.
    /// </summary>
    /// <param name="dto">The booking request data to be processed.</param>
    /// <returns>A structured response containing the outcome of the booking operation.</returns>
    public async Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto)
    {
        var sanitizedName = dto.FullName?
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty);

        _logger.LogDebug("Starting creation of booking request for customer: {CustomerName}", sanitizedName);

        BookingRequest bookingRequest = _mapper.Map<BookingRequest>(dto);

        await _repository.AddAsync(bookingRequest);

        _logger.LogInformation("Successfully processed and saved booking request. Generated ID: {BookingId}", bookingRequest.Id);

        return new BookingResponseDTO(
            bookingRequest.Id,
            "Booking request submitted successfully.",
            bookingRequest.Status.ToString(),
            bookingRequest.CreatedAt
        );
    }

    #endregion
}
