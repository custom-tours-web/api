namespace api.DTOs;

/// <summary>
/// Represents the options for configuring email notifications for booking requests, including SMTP settings and sender/recipient information.
/// </summary>
public class BookingEmailOptionsDTO
{

    /// <summary>
    /// Gets or sets the SMTP host address used for sending email notifications.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port number used for sending email notifications. The default value is 587, which is commonly used for secure email transmission.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username used for authenticating with the SMTP server when sending email notifications.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password used for authenticating with the SMTP server when sending email notifications.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address from which booking notification emails will be sent. This address is used as the "From" address in the email headers.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name associated with the "From" email address.
    /// </summary>
    public string FromName { get; set; } = "Custom Tours";

    /// <summary>
    /// Gets or sets the email address to which booking notification emails will be sent. This address is used as the "To" address in the email headers.
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;
}
