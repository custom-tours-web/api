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
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            CurrentLocation = dto.CurrentLocation,
            Destination = dto.Destination,
            TourDate = dto.TourDate,
            NumberOfMembers = dto.NumberOfMembers,
            SpecialRequests = dto.SpecialRequests,
            Status = BookingRequestStatus.Pending
        };
    }
}
