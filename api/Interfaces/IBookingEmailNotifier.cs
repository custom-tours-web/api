using api.Models;

namespace api.Interfaces;

public interface IBookingEmailNotifier
{
    Task SendBookingNotificationAsync(BookingRequest bookingRequest);
}
