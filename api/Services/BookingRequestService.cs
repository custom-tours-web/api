using api.DTOs;
using api.Extensions;
using api.Interfaces;

namespace api.Services;

public class BookingRequestService(
    IBookingRequestRepository repository) : IBookingRequestService
{
    private readonly IBookingRequestRepository _repository = repository;

    public async Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto)
    {
        var bookingRequest = dto.ToEntity();
        await _repository.AddAsync(bookingRequest);
        return new BookingResponseDTO(
            bookingRequest.Id,
            "Booking request submitted successfully.",
            bookingRequest.Status.ToString(),
            bookingRequest.CreatedAt
        );
    }
}
