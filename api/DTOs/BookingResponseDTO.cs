namespace api.DTOs;

/// <summary>
/// Data Transfer Object representing the outcome of a booking request operation.
/// </summary>
/// <param name="Id">The unique identifier of the newly created booking request.</param>
/// <param name="Message">A human-readable message describing the result of the operation.</param>
/// <param name="Status">The current status of the booking (e.g., "Pending", "Confirmed", "Rejected").</param>
/// <param name="CreatedAt">The UTC timestamp indicating when the booking response was generated.</param>
public record BookingResponseDTO(
    int Id,
    string Message,
    string Status,
    DateTimeOffset CreatedAt
);
