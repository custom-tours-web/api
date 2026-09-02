namespace api.Models.Enums;

#region Enum Definitions

/// <summary>
/// Represents the current lifecycle status of a booking request.
/// </summary>
public enum BookingRequestStatus
{
    /// <summary>
    /// The booking request has been received but not yet processed or reviewed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// A representative has contacted the user regarding the booking request.
    /// </summary>
    Contacted = 1,

    /// <summary>
    /// The booking request has been finalized and confirmed.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// The booking request was cancelled by the user or the system.
    /// </summary>
    Cancelled = 3
}

#endregion
