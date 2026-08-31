using api.Interfaces;
using api.Models;

namespace api.Repositories;

public class BookingRequestRepository() : IBookingRequestRepository
{
    // private readonly AppDbContext _context = context;

    public async Task AddAsync(BookingRequest request)
    {
        // _context.BookingRequests.Add(request);
        // await _context.SaveChangesAsync();
    }
}
