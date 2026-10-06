using api.Models;

namespace api.Interfaces;

public interface IWhatsAppNotifier
{
    Task SendBookingNotificationAsync(BookingRequest bookingRequest);
}
