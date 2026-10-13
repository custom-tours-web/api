using api.DTOs;
using api.Interfaces;
using api.Models;
using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace api.Services;

/// <summary>
/// Represents a service responsible for handling booking requests, including creating new requests and sending notifications via WhatsApp and email.
/// </summary>
/// <param name="mapper">The AutoMapper instance for mapping between DTOs & domain entities.</param>
/// <param name="repository">The repository instance for performing data access operations related to booking requests.</param>
/// <param name="whatsAppNotifier">The notifier used to send booking details over WhatsApp.</param>
/// <param name="bookingEmailNotifier">The notifier used to send booking details by email.</param>
/// <param name="notificationFeatures">The configuration options that determine which notification channels are enabled for booking requests.</param>
/// <param name="logger">The logger instance for logging service-level operations & errors.</param>
public class BookingRequestService(
    IMapper mapper,
    IBookingRequestRepository repository,
    IWhatsAppNotifier whatsAppNotifier,
    IBookingEmailNotifier bookingEmailNotifier,
    IOptions<BookingNotificationFeaturesDTO> notificationFeatures,
    ILogger<BookingRequestService> logger) : IBookingRequestService
{
    #region Dependencies

    /// <summary>
    /// The AutoMapper instance for mapping between DTOs & domain entities.
    /// </summary>
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// The logger instance for logging service-level operations & errors.
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

    /// <summary>
    /// The configuration options that determine which notification channels are enabled for booking requests.
    /// </summary>
    private readonly BookingNotificationFeaturesDTO _notificationFeatures = notificationFeatures.Value;

    #endregion

    #region Business Logic

    /// <summary>
    /// Asynchronously processes a booking request by mapping the DTO to an entity & persisting it.
    /// </summary>
    /// <param name="dto">The booking request data to be processed.</param>
    /// <returns>A structured response containing the outcome of the booking operation.</returns>
    public async Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto)
    {
        var sanitizedName = dto.FullName?.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", string.Empty, StringComparison.Ordinal);

        _logger.LogDebug("Starting creation of booking request for customer: {CustomerName}", sanitizedName);

        BookingRequest bookingRequest = _mapper.Map<BookingRequest>(dto);

        await _repository.AddAsync(bookingRequest);

        _logger.LogInformation("Successfully processed & saved booking request. Generated ID: {BookingId}", bookingRequest.Id);

        var notificationTasks = new List<Task>(2);
        if (_notificationFeatures.WhatsAppEnabled) notificationTasks.Add(SendWhatsAppNotificationSafelyAsync(bookingRequest));
        if (_notificationFeatures.EmailEnabled) notificationTasks.Add(SendEmailNotificationSafelyAsync(bookingRequest));

        await Task.WhenAll(notificationTasks);

        return new BookingResponseDTO(bookingRequest.Id, "Booking request submitted successfully.", bookingRequest.Status.ToString(), DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Safely sends a WhatsApp notification for a booking request, logging any exceptions that occur during the process.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send a WhatsApp notification.</param>
    /// <returns></returns>
    private async Task SendWhatsAppNotificationSafelyAsync(BookingRequest bookingRequest)
    {
        try
        {
            await _whatsAppNotifier.SendBookingNotificationAsync(bookingRequest);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Booking request {BookingId} was saved, but its WhatsApp notification could not be delivered.", bookingRequest.Id);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(exception, "Booking request {BookingId} was saved, but its WhatsApp notification timed out.", bookingRequest.Id);
        }
    }

    /// <summary>
    /// Safely sends an email notification for a booking request, logging any exceptions that occur during the process.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send an email notification.</param>
    /// <returns></returns>
    private async Task SendEmailNotificationSafelyAsync(BookingRequest bookingRequest)
    {
        try
        {
            await _bookingEmailNotifier.SendBookingNotificationAsync(bookingRequest);
        }
        catch (SmtpException exception)
        {
            _logger.LogError(exception, "Booking request {BookingId} was saved, but its email notification could not be delivered.", bookingRequest.Id);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(exception, "Booking request {BookingId} was saved, but its email notification timed out.", bookingRequest.Id);
        }
    }

    #endregion
}
