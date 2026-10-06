namespace api.Services;

public class MetaWhatsAppOptions
{
    public string AccessToken { get; set; } = string.Empty;

    public string PhoneNumberId { get; set; } = string.Empty;

    public string WhatsAppTo { get; set; } = string.Empty;

    public string GraphApiVersion { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string TemplateLanguageCode { get; set; } = string.Empty;
}
