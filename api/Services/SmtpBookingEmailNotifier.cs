using System.Globalization;
using System.Net.Mail;
using System.Text;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Services;

public class SmtpBookingEmailNotifier(
    IOptions<BookingEmailOptions> options,
    IBookingEmailTransport emailTransport,
    ILogger<SmtpBookingEmailNotifier> logger) : IBookingEmailNotifier
{
    private readonly BookingEmailOptions _options = options.Value;
    private readonly IBookingEmailTransport _emailTransport = emailTransport;
    private readonly ILogger<SmtpBookingEmailNotifier> _logger = logger;

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

        await _emailTransport.SendAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            new System.Net.NetworkCredential(_options.SmtpUsername, _options.SmtpPassword),
            message);
    }

    private static string FormatBookingMessage(BookingRequest bookingRequest) =>
        string.Join(
            Environment.NewLine,
            $"New booking request #{bookingRequest.Id}",
            $"Name: {SingleLine(bookingRequest.FullName)}",
            $"Phone: {SingleLine(bookingRequest.PhoneNumber)}",
            $"From: {SingleLine(bookingRequest.CurrentLocation)}",
            $"Destination: {SingleLine(bookingRequest.Destination)}",
            string.Format(
                CultureInfo.InvariantCulture,
                "Dates: {0:yyyy-MM-dd} to {1:yyyy-MM-dd}",
                bookingRequest.FromDate,
                bookingRequest.ToDate),
            string.Format(CultureInfo.InvariantCulture, "Members: {0}", bookingRequest.NumberOfMembers),
            $"Special requests: {SingleLine(bookingRequest.SpecialRequests ?? "None")}");

    private static string SingleLine(string value) =>
        value.Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
}
