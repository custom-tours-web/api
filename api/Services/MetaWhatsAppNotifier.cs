using System.Globalization;
using System.Net.Http.Headers;
using api.DTOs;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Services;

/// <summary>
/// Provides an implementation of the IWhatsAppNotifier interface for sending notifications via the Meta (Facebook) Graph API.
/// </summary>
/// <param name="httpClient">The HttpClient instance used for sending HTTP requests.</param>
/// <param name="options">The configuration options for the Meta WhatsApp notifier.</param>
/// <param name="logger">The logger instance for logging information, warnings, and errors.</param>
public class MetaWhatsAppNotifier(HttpClient httpClient, IOptions<MetaWhatsAppOptionsDTO> options, ILogger<MetaWhatsAppNotifier> logger) : IWhatsAppNotifier
{

    /// <summary>
    /// The HttpClient instance used for sending HTTP requests to the Meta (Facebook) Graph API.
    /// </summary>
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// The configuration options for sending WhatsApp notifications
    /// </summary>
    private readonly MetaWhatsAppOptionsDTO _options = options.Value;

    /// <summary>
    /// The logger instance for logging information, warnings, and errors related to the WhatsApp notification process.
    /// </summary>
    private readonly ILogger<MetaWhatsAppNotifier> _logger = logger;

    /// <summary>
    /// Sends a WhatsApp notification regarding a new booking request using the Meta (Facebook) Graph API.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send a notification.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the booking request is invalid.</exception>
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

        var recipient = _options.WhatsAppTo.StartsWith('+') ? _options.WhatsAppTo[1..] : _options.WhatsAppTo;
        if (recipient.Length == 0 || !recipient.All(char.IsAsciiDigit))
            throw new InvalidOperationException("Meta WhatsApp recipient must be an international phone number containing digits only, optionally prefixed with '+'.");

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

    /// <summary>
    /// Sanitizes a string to ensure it is a single line by replacing line breaks with spaces and trimming whitespace.
    /// </summary>
    /// <param name="value">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    private static string SingleLine(string value) => value.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();
}
