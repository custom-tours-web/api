using api.DTOs;

namespace api.Interfaces;

public interface IBookingRequestService
{
    Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto);
}
