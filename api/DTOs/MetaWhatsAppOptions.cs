namespace api.DTOs;

/// <summary>
/// Represents configuration options for sending WhatsApp notifications via the Meta (Facebook) Graph API.
/// </summary>
public class MetaWhatsAppOptionsDTO
{

    /// <summary>
    /// Gets or sets the access token used for authenticating with the Meta Graph API. This token is required to send WhatsApp messages through the API.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the WhatsApp Business phone number associated with the Meta Graph API.
    /// </summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient's WhatsApp phone number in international format (e.g., +1234567890) to which the booking notifications will be sent.
    /// </summary>
    public string WhatsAppTo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the Meta Graph API to be used for sending WhatsApp messages. This allows for compatibility with specific API features and endpoints.
    /// </summary>
    public string GraphApiVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the message template to be used for sending WhatsApp notifications.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language code for the message template, which determines the language in which the WhatsApp notification will be sent.
    /// </summary>
    public string TemplateLanguageCode { get; set; } = string.Empty;
}
