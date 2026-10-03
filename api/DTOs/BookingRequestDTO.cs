using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

/// <summary>
/// Data Transfer Object representing an incoming request to create a booking.
/// </summary>
/// <param name="FullName">The full name of the person making the booking.</param>
/// <param name="PhoneNumber">A valid contact phone number.</param>
/// <param name="CurrentLocation">The starting location of the tour.</param>
/// <param name="Destination">The intended destination for the tour.</param>
/// <param name="FromDate">The starting date of the tour. Must be in the future.</param>
/// <param name="ToDate">The ending date of the tour. Must be on or after the starting date.</param>
/// <param name="NumberOfMembers">The total number of people participating in the tour (1-100).</param>
/// <param name="SpecialRequests">Any optional special requests or accommodations.</param>
public record BookingRequestDTO(
    [Required, MaxLength(100)] string FullName,
    [Required, Phone] string PhoneNumber,
    [Required, MaxLength(150)] string CurrentLocation,
    [Required, MaxLength(150)] string Destination,
    [Required] DateOnly FromDate,
    [Required] DateOnly ToDate,
    [Required, Range(1, 100)] int NumberOfMembers,
    [MaxLength(500)] string? SpecialRequests
) : IValidatableObject
{
    #region Validation Logic

    /// <summary>
    /// Performs custom validation rules that cannot be captured by standard data annotations.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating whether the object is valid.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate that the start and end destinations are distinct locations
        if (string.Equals(CurrentLocation, Destination, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Current Location and Destination cannot be the same.",
                [nameof(CurrentLocation), nameof(Destination)]
            );
        }

        // Validate that the tour date is strictly in the future
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (FromDate <= today)
        {
            yield return new ValidationResult(
                "Tour start date must be in the future.",
                [nameof(FromDate)]
            );
        }

        if (ToDate < FromDate)
        {
            yield return new ValidationResult(
                "Tour end date cannot be before the start date.",
                [nameof(ToDate)]
            );
        }
    }

    #endregion
}
