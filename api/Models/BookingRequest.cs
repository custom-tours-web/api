using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Models.Enums;

namespace api.Models;

/// <summary>
/// Represents a booking request entity in the database.
/// </summary>
public class BookingRequest
{
    #region Primary Key

    /// <summary>
    /// The unique identifier for the booking request.
    /// </summary>
    [Key]
    public int Id { get; set; }

    #endregion

    #region Customer & Tour Details

    /// <summary>
    /// The full name of the customer making the request.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The contact phone number for the customer.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The starting location or current city of the customer.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string CurrentLocation { get; set; } = string.Empty;

    /// <summary>
    /// The desired destination for the tour.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// The specific date the customer wishes to take the tour.
    /// </summary>
    [Required]
    public DateOnly TourDate { get; set; }

    /// <summary>
    /// The total number of people participating in the tour.
    /// </summary>
    [Required]
    public int NumberOfMembers { get; set; }

    /// <summary>
    /// Any optional special requests or accommodations required by the customer.
    /// </summary>
    [MaxLength(500)]
    public string? SpecialRequests { get; set; }

    #endregion

    #region Status & Audit Metadata

    /// <summary>
    /// The current processing status of the booking request. Defaults to Pending.
    /// </summary>
    public BookingRequestStatus Status { get; set; } = BookingRequestStatus.Pending;

    /// <summary>
    /// The timestamp indicating when the record was initially created in the database.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// The timestamp indicating when the record was last modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    #endregion
}
