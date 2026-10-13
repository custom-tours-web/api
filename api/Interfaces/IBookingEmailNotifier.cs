using api.Models;

namespace api.Interfaces;

/// <summary>
/// Defines a contract for sending email notifications related to booking requests.
/// </summary>
public interface IBookingEmailNotifier
{

    /// <summary>
    /// Sends an email notification regarding a new booking request.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send notification.</param>
    Task SendBookingNotificationAsync(BookingRequest bookingRequest);
}
