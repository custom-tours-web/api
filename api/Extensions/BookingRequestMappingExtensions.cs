using api.DTOs;
using api.Models;
using api.Models.Enums;

namespace api.Extensions;

public static class BookingRequestMappingExtensions
{
    public static BookingRequest ToEntity(this BookingRequestDTO dto)
    {
        return new BookingRequest
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Status = BookingRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
