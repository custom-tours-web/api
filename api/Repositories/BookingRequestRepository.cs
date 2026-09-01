using api.Datas;
using api.Interfaces;
using api.Models;

namespace api.Repositories;

public class BookingRequestRepository(TourismDbContext context) : IBookingRequestRepository
{
    private readonly TourismDbContext _context = context;

    public async Task AddAsync(BookingRequest request)
    {
        await _context.BookingRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }
}
