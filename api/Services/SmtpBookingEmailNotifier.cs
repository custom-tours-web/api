using System.Globalization;
using System.Net.Mail;
using System.Text;
using api.DTOs;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Services;

/// <summary>
/// Provides an implementation of the IBookingEmailNotifier interface for sending email notifications regarding booking requests using SMTP.
/// </summary>
/// <param name="options">The configuration options for sending booking email notifications.</param>
/// <param name="emailTransport">The transport mechanism for sending emails.</param>
/// <param name="logger">The logger instance for logging information, warnings, and errors.</param>
public class SmtpBookingEmailNotifier( IOptions<BookingEmailOptionsDTO> options, IBookingEmailTransport emailTransport, ILogger<SmtpBookingEmailNotifier> logger)
 : IBookingEmailNotifier
{
    /// <summary>
    /// The configuration options for sending booking email notifications, including SMTP server details and email addresses.
    /// </summary>
    private readonly BookingEmailOptionsDTO _options = options.Value;

    /// <summary>
    /// The transport mechanism for sending emails, which abstracts the underlying SMTP implementation.
    /// </summary>
    private readonly IBookingEmailTransport _emailTransport = emailTransport;

    /// <summary>
    /// The logger instance for logging information, warnings, and errors related to the email notification process.
    /// </summary>
    private readonly ILogger<SmtpBookingEmailNotifier> _logger = logger;

    /// <summary>
    /// Sends an email notification regarding a new booking request using the configured SMTP settings.
    /// </summary>
    /// <param name="bookingRequest">The booking request for which to send notification.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost)
            || string.IsNullOrWhiteSpace(_options.SmtpUsername)
            || string.IsNullOrWhiteSpace(_options.SmtpPassword)
            || string.IsNullOrWhiteSpace(_options.FromAddress)
            || string.IsNullOrWhiteSpace(_options.ToAddress))
        {
            _logger.LogWarning("Booking email notifications are disabled because SMTP configuration is incomplete.");
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = $"New booking request #{bookingRequest.Id}",
            Body = FormatBookingMessage(bookingRequest),
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(_options.ToAddress));

        await _emailTransport.SendAsync( _options.SmtpHost, _options.SmtpPort, new System.Net.NetworkCredential(_options.SmtpUsername, _options.SmtpPassword), message);
    }

    /// <summary>
    /// Formats the booking request details into a structured message string for email notifications.
    /// </summary>
    /// <param name="bookingRequest">The booking request to format.</param>
    /// <returns> The formatted message string.</returns>
    private static string FormatBookingMessage(BookingRequest bookingRequest) =>
        string.Join(
            Environment.NewLine,
            $"New booking request #{bookingRequest.Id}",
            $"Name: {SingleLine(bookingRequest.FullName)}",
            $"Phone: {SingleLine(bookingRequest.PhoneNumber)}",
            $"From: {SingleLine(bookingRequest.CurrentLocation)}",
            $"Destination: {SingleLine(bookingRequest.Destination)}",
            string.Format(CultureInfo.InvariantCulture, "Dates: {0:yyyy-MM-dd} to {1:yyyy-MM-dd}", bookingRequest.FromDate, bookingRequest.ToDate),
            string.Format(CultureInfo.InvariantCulture, "Members: {0}", bookingRequest.NumberOfMembers), $"Special requests: {SingleLine(bookingRequest.SpecialRequests ?? "None")}");

    /// <summary>
    /// Sanitizes a string to ensure it is a single line by replacing line breaks with spaces and trimming whitespace.
    /// </summary>
    /// <param name="value">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    private static string SingleLine(string value) =>
    value.Replace("\r\n", " ", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();
}
