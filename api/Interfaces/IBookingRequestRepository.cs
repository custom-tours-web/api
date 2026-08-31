using api.Models;

namespace api.Interfaces;

public interface IBookingRequestRepository
{
    Task AddAsync(BookingRequest request);
}
