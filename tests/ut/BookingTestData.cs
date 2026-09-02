using api.DTOs;
using api.Models;
using api.Models.Enums;

namespace ut;

/// <summary>
/// Provides centralized, valid test data for unit tests to prevent duplication.
/// </summary>
public static class BookingTestData
{
    public static BookingRequestDTO GetValidBookingRequestDTO() =>
        new(
            "Jane Doe",
            "555-1234",
            "New York",
            "Los Angeles",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            2,
            "Window seat preferred"
        );

    public static BookingResponseDTO GetValidBookingResponseDTO(int id = 1) =>
        new(
            id,
            "Booking request submitted successfully.",
            BookingRequestStatus.Pending.ToString(),
            DateTimeOffset.UtcNow
        );

    public static BookingRequest GetValidBookingRequestEntity(int id = 1) =>
        new()
        {
            Id = id,
            FullName = "Jane Doe",
            PhoneNumber = "555-1234",
            CurrentLocation = "New York",
            Destination = "Los Angeles",
            TourDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            NumberOfMembers = 2,
            SpecialRequests = "Window seat preferred",
            Status = BookingRequestStatus.Pending,
        };
}
