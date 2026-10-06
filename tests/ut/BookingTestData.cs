using api.DTOs;
using api.Models;
using api.Models.Enums;

namespace ut;

public static class BookingTestData
{
    public static BookingRequestDTO GetValidBookingRequestDTO() => new(
        FullName: "Jane Smith",
        PhoneNumber: "+1234567890",
        CurrentLocation: "New York",
        Destination: "Paris",
        FromDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        ToDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
        NumberOfMembers: 2,
        SpecialRequests: "Vegetarian meals"
    );

    public static BookingRequest GetValidBookingRequestEntity() => new()
    {
        Id = 123,
        FullName = "Jane Smith",
        PhoneNumber = "+1234567890",
        CurrentLocation = "New York",
        Destination = "Paris",
        FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        ToDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
        NumberOfMembers = 2,
        SpecialRequests = "Vegetarian meals",
        Status = BookingRequestStatus.Pending,
        UpdatedAt = null
    };

    public static BookingResponseDTO GetValidBookingResponseDTO() => new(
        Id: 123,
        Message: "Booking request submitted successfully.",
        Status: BookingRequestStatus.Pending.ToString(),
        CreatedAt: DateTimeOffset.UtcNow
    );
}
