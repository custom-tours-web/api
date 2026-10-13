using api.Models;

namespace api.Interfaces;

/// <summary>
/// Defines a contract for sending WhatsApp notifications related to booking requests.
/// </summary>
public interface IWhatsAppNotifier
{

    /// <summary>
    /// Sends a WhatsApp notification regarding a new booking request.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send a notification.</param>
    Task SendBookingNotificationAsync(BookingRequest bookingRequest);
}
