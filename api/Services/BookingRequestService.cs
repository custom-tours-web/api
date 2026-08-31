using api.DTOs;
using api.Interfaces;

namespace api.Services;

public class BookingRequestService : IBookingRequestService
{
    private readonly IBookingRequestRepository _repository;
    private readonly IBookingRequestMapper _mapper;

    public BookingRequestService(
        IBookingRequestRepository repository,
        IBookingRequestMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto)
    {
        // 1. Delegate mapping to the mapper class
        var bookingRequest = _mapper.ToEntity(dto);

        // 2. Persist the entity via the Repository
        await _repository.AddAsync(bookingRequest);

        // 3. Return a clean response format
        return new BookingResponseDTO(
            bookingRequest.Id,
            "Booking request submitted successfully.",
            bookingRequest.Status.ToString(),
            bookingRequest.CreatedAt
        );
    }
}
