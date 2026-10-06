using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Services;

public class MetaWhatsAppNotifier(
    HttpClient httpClient,
    IOptions<MetaWhatsAppOptions> options,
    ILogger<MetaWhatsAppNotifier> logger) : IWhatsAppNotifier
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly MetaWhatsAppOptions _options = options.Value;
    private readonly ILogger<MetaWhatsAppNotifier> _logger = logger;

    public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken)
            || string.IsNullOrWhiteSpace(_options.PhoneNumberId)
            || string.IsNullOrWhiteSpace(_options.WhatsAppTo)
            || string.IsNullOrWhiteSpace(_options.GraphApiVersion)
            || string.IsNullOrWhiteSpace(_options.TemplateName)
            || string.IsNullOrWhiteSpace(_options.TemplateLanguageCode))
        {
            _logger.LogWarning("Meta WhatsApp notifications are disabled because configuration is incomplete.");
            return;
        }

        var recipient = _options.WhatsAppTo.StartsWith('+')
            ? _options.WhatsAppTo[1..]
            : _options.WhatsAppTo;
        if (recipient.Length == 0 || !recipient.All(char.IsAsciiDigit))
        {
            throw new InvalidOperationException(
                "Meta WhatsApp recipient must be an international phone number containing digits only, optionally prefixed with '+'.");
        }

        var parameters = new[]
        {
            bookingRequest.Id.ToString(CultureInfo.InvariantCulture),
            SingleLine(bookingRequest.FullName),
            SingleLine(bookingRequest.PhoneNumber),
            SingleLine(bookingRequest.CurrentLocation),
            SingleLine(bookingRequest.Destination),
            bookingRequest.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bookingRequest.ToDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bookingRequest.NumberOfMembers.ToString(CultureInfo.InvariantCulture),
            SingleLine(bookingRequest.SpecialRequests ?? "None")
        };

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = recipient,
            type = "template",
            template = new
            {
                name = _options.TemplateName,
                language = new { code = _options.TemplateLanguageCode },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters.Select(text => new { type = "text", text })
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://graph.facebook.com/{Uri.EscapeDataString(_options.GraphApiVersion)}/{Uri.EscapeDataString(_options.PhoneNumberId)}/messages")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static string SingleLine(string value) =>
        value.Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
}
