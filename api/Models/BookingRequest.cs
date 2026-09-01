using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Models.Enums;

namespace api.Models;

public class BookingRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CurrentLocation { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public DateOnly TourDate { get; set; }

    [Required]
    public int NumberOfMembers { get; set; }

    [MaxLength(500)]
    public string? SpecialRequests { get; set; }

    public BookingRequestStatus Status { get; set; } = BookingRequestStatus.Pending;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
