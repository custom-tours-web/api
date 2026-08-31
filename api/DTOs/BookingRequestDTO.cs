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
);
