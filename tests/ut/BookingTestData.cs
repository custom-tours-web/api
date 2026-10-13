using api.DTOs;
using api.Models;
using api.Models.Enums;

namespace ut;

/// <summary>
/// It provides test data for booking requests, including valid DTOs & entities, to be used in unit tests for controllers & repositories.
/// </summary>
public static class BookingTestData
{
    /// <summary>
    /// It returns a valid BookingRequestDTO object with sample data, which can be used in unit tests to simulate a booking request submission.
    /// </summary>
    /// <returns>booking request DTO</returns>
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

    /// <summary>
    /// It returns a valid BookingRequest entity object with sample data, which can be used in unit tests to simulate a booking request stored in the database.
    /// </summary>
    /// <returns>booking request entity</returns>
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

    /// <summary>
    /// It returns a valid BookingResponseDTO object with sample data, which can be used in unit tests to simulate the response
    /// </summary>
    /// <returns>booking response DTO</returns>
    public static BookingResponseDTO GetValidBookingResponseDTO() => new(
        Id: 123,
        Message: "Booking request submitted successfully.",
        Status: BookingRequestStatus.Pending.ToString(),
        CreatedAt: DateTimeOffset.UtcNow
    );
}
