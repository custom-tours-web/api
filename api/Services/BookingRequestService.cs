using api.DTOs;
using api.Extensions;
using api.Interfaces;
using api.Models;
using AutoMapper;

namespace api.Services;

public class BookingRequestService(IMapper mapper, IConfiguration configuration,
    IBookingRequestRepository repository) : IBookingRequestService
{
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _configuration = configuration;
    private readonly IBookingRequestRepository _repository = repository;

    public async Task<BookingResponseDTO> CreateBookingRequestAsync(BookingRequestDTO dto)
    {
        BookingRequest bookingRequest = _mapper.Map<BookingRequest>(dto);
        await _repository.AddAsync(bookingRequest);
        return new BookingResponseDTO(
            bookingRequest.Id,
            "Booking request submitted successfully.",
            bookingRequest.Status.ToString(),
            bookingRequest.CreatedAt
        );
    }
}
