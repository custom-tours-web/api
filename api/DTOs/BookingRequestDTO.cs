using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public record BookingRequestDTO(
    [Required, MaxLength(100)] string FullName,
    [Required, Phone] string PhoneNumber,
    [Required, MaxLength(150)] string CurrentLocation,
    [Required, MaxLength(150)] string Destination,
    [Required] DateOnly TourDate,
    [Required, Range(1, 100)] int NumberOfMembers,
    [MaxLength(500)] string? SpecialRequests
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(CurrentLocation, Destination, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Current Location and Destination cannot be the same.",
                [nameof(CurrentLocation), nameof(Destination)]
            );
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (TourDate <= today)
        {
            yield return new ValidationResult(
                "Tour date must be in the future.",
                [nameof(TourDate)]
            );
        }
    }
}
