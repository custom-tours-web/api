namespace api.DTOs;

/// <summary>
/// Represents the features available for booking notifications, indicating whether WhatsApp and email notifications are enabled.
/// </summary>
public class BookingNotificationFeaturesDTO
{

    /// <summary>
    /// Gets or sets a value indicating whether WhatsApp notifications are enabled for booking requests.
    /// </summary>
    public bool WhatsAppEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether email notifications are enabled for booking requests.
    /// </summary>
    public bool EmailEnabled { get; set; }
}
