using api.DTOs;
using api.Interfaces;
using api.Models;
using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Mail;

namespace api.Services;

/// <summary>
/// Orchestrates data mapping, persistence via the repository, and returning standardized responses.
/// </summary>
public class BookingRequestService(
    IMapper mapper,
    IBookingRequestRepository repository,
    IWhatsAppNotifier whatsAppNotifier,
    IBookingEmailNotifier bookingEmailNotifier,
    IOptions<BookingNotificationFeatures> notificationFeatures,
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

    /// <summary>
    /// The notifier used to send booking details over WhatsApp.
    /// </summary>
    private readonly IWhatsAppNotifier _whatsAppNotifier = whatsAppNotifier;

    /// <summary>
    /// The notifier used to send booking details by email.
    /// </summary>
    private readonly IBookingEmailNotifier _bookingEmailNotifier = bookingEmailNotifier;

    private readonly BookingNotificationFeatures _notificationFeatures = notificationFeatures.Value;

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
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);

        _logger.LogDebug("Starting creation of booking request for customer: {CustomerName}", sanitizedName);

        BookingRequest bookingRequest = _mapper.Map<BookingRequest>(dto);

        await _repository.AddAsync(bookingRequest);

        _logger.LogInformation("Successfully processed and saved booking request. Generated ID: {BookingId}", bookingRequest.Id);

        var notificationTasks = new List<Task>(2);
        if (_notificationFeatures.WhatsAppEnabled)
            notificationTasks.Add(SendWhatsAppNotificationSafelyAsync(bookingRequest));
        if (_notificationFeatures.EmailEnabled)
            notificationTasks.Add(SendEmailNotificationSafelyAsync(bookingRequest));

        await Task.WhenAll(notificationTasks);

        return new BookingResponseDTO(
            bookingRequest.Id,
            "Booking request submitted successfully.",
            bookingRequest.Status.ToString(),
            DateTimeOffset.UtcNow
        );
    }

    private async Task SendWhatsAppNotificationSafelyAsync(BookingRequest bookingRequest)
    {
        try
        {
            await _whatsAppNotifier.SendBookingNotificationAsync(bookingRequest);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Booking request {BookingId} was saved, but its WhatsApp notification could not be delivered.",
                bookingRequest.Id);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(
                exception,
                "Booking request {BookingId} was saved, but its WhatsApp notification timed out.",
                bookingRequest.Id);
        }
    }

    private async Task SendEmailNotificationSafelyAsync(BookingRequest bookingRequest)
    {
        try
        {
            await _bookingEmailNotifier.SendBookingNotificationAsync(bookingRequest);
        }
        catch (SmtpException exception)
        {
            _logger.LogError(
                exception,
                "Booking request {BookingId} was saved, but its email notification could not be delivered.",
                bookingRequest.Id);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(
                exception,
                "Booking request {BookingId} was saved, but its email notification timed out.",
                bookingRequest.Id);
        }
    }

    #endregion
}
