using System.Net;
using System.Net.Mail;
using api.DTOs;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ut.Services;

/// <summary>
/// Unit tests for the SmtpBookingEmailNotifier class, which is responsible for sending booking notification emails via SMTP.
/// </summary>
[TestFixture]
public class SmtpBookingEmailNotifierTests
{

    /// <summary>
    /// Tests that the SendBookingNotificationAsync method sends an email with the correct booking details to the configured recipient address.
    /// </summary>
    [Test]
    public async Task SendBookingNotificationAsync_SendsBookingDetailsToConfiguredAddress()
    {
        var transport = new CapturingEmailTransport();
        var notifier = new SmtpBookingEmailNotifier(
            Options.Create(new BookingEmailOptionsDTO
            {
                SmtpHost = "smtp.example.test",
                SmtpPort = 587,
                SmtpUsername = "smtp-user",
                SmtpPassword = "smtp-password",
                FromAddress = "bookings@example.test",
                FromName = "Custom Tours",
                ToAddress = "naren000000000@gmail.com"
            }),
            transport,
            NullLogger<SmtpBookingEmailNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest
        {
            Id = 19,
            FullName = "Ada Lovelace",
            PhoneNumber = "+441234567890",
            CurrentLocation = "London\r\nInjected: value",
            Destination = "Pune",
            FromDate = new DateOnly(2027, 1, 2),
            ToDate = new DateOnly(2027, 1, 9),
            NumberOfMembers = 3,
            SpecialRequests = "Window seat"
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(transport.Host, Is.EqualTo("smtp.example.test"));
            Assert.That(transport.Port, Is.EqualTo(587));
            Assert.That(transport.Username, Is.EqualTo("smtp-user"));
            Assert.That(transport.FromAddress, Is.EqualTo("bookings@example.test"));
            Assert.That(transport.FromName, Is.EqualTo("Custom Tours"));
            Assert.That(transport.ToAddress, Is.EqualTo("naren000000000@gmail.com"));
            Assert.That(transport.Subject, Is.EqualTo("New booking request #19"));
            Assert.That(transport.IsBodyHtml, Is.False);
            Assert.That(transport.Body, Does.Contain("New booking request #19"));
        }
        Assert.That(transport.Body, Does.Contain("London Injected: value"));
        Assert.That(transport.Body, Does.Contain("Name: Ada Lovelace"));
        Assert.That(transport.Body, Does.Contain("Phone: +441234567890"));
        Assert.That(transport.Body, Does.Contain("Destination: Pune"));
        Assert.That(transport.Body, Does.Contain("Dates: 2027-01-02 to 2027-01-09"));
        Assert.That(transport.Body, Does.Contain("Members: 3"));
        Assert.That(transport.Body, Does.Contain("Special requests: Window seat"));
        Assert.That(transport.Body, Does.Not.Contain("\r\nInjected:"));
    }

    /// <summary>
    /// Tests that the SendBookingNotificationAsync method correctly flattens standalone carriage return & line feed characters in the booking request.
    /// </summary>
    [Test]
    public async Task SendBookingNotificationAsync_FlattensStandaloneCarriageReturnsAndLineFeeds()
    {
        var transport = new CapturingEmailTransport();
        var notifier = CreateNotifier(transport);

        await notifier.SendBookingNotificationAsync(new BookingRequest
        {
            FullName = "Ada\nLovelace",
            PhoneNumber = "123\r456"
        });

        Assert.That(transport.Body, Does.Contain("Name: Ada Lovelace"));
        Assert.That(transport.Body, Does.Contain("Phone: 123 456"));
    }

    /// <summary>
    /// Tests that the SendBookingNotificationAsync method uses default text for special requests when the SpecialRequests property of the BookingRequest is null.
    /// </summary>
    [Test]
    public async Task SendBookingNotificationAsync_WhenSpecialRequestsAreMissing_UsesDefaultText()
    {
        var transport = new CapturingEmailTransport();
        var notifier = CreateNotifier(transport);

        await notifier.SendBookingNotificationAsync(new BookingRequest { SpecialRequests = null });

        Assert.That(transport.Body, Does.Contain("Special requests: None"));
    }

    /// <summary>
    /// Tests that the SendBookingNotificationAsync method does not send an email when any required SMTP setting is missing.
    /// </summary>
    /// <param name="missingSetting"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [TestCase("SmtpHost")]
    [TestCase("SmtpUsername")]
    [TestCase("SmtpPassword")]
    [TestCase("FromAddress")]
    [TestCase("ToAddress")]
    public async Task SendBookingNotificationAsync_WhenARequiredSmtpSettingIsMissing_DoesNotSend(string missingSetting) {
        var transport = new CapturingEmailTransport();
        var options = CreateCompleteOptions();
        switch (missingSetting)
        {
            case "SmtpHost":
                options.SmtpHost = string.Empty;
                break;
            case "SmtpUsername":
                options.SmtpUsername = string.Empty;
                break;
            case "SmtpPassword":
                options.SmtpPassword = string.Empty;
                break;
            case "FromAddress":
                options.FromAddress = string.Empty;
                break;
            case "ToAddress":
                options.ToAddress = string.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(missingSetting));
        }

        var notifier = new SmtpBookingEmailNotifier(Options.Create(options), transport, NullLogger<SmtpBookingEmailNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest());

        Assert.That(transport.SendCount, Is.Zero);
    }

    /// <summary>
    /// Tests that the BookingEmailOptions class has the expected default values for its properties.
    /// </summary>
    [Test]
    public void BookingEmailOptions_HaveExpectedDefaults()
    {
        var options = new BookingEmailOptionsDTO();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(options.SmtpHost, Is.Empty);
            Assert.That(options.SmtpPort, Is.EqualTo(587));
            Assert.That(options.SmtpUsername, Is.Empty);
            Assert.That(options.SmtpPassword, Is.Empty);
            Assert.That(options.FromAddress, Is.Empty);
            Assert.That(options.FromName, Is.EqualTo("Custom Tours"));
            Assert.That(options.ToAddress, Is.Empty);
        }
    }

    /// <summary>
    /// Creates an instance of SmtpBookingEmailNotifier with the provided CapturingEmailTransport & complete BookingEmailOptions.
    /// </summary>
    /// <param name="transport">The capturing email transport to use.</param>
    /// <returns></returns>
    private static SmtpBookingEmailNotifier CreateNotifier(CapturingEmailTransport transport) => new(Options.Create(CreateCompleteOptions()), transport, NullLogger<SmtpBookingEmailNotifier>.Instance);

    /// <summary>
    /// Creates a BookingEmailOptions object with all required properties set to valid test values.
    /// </summary>
    /// <returns></returns>
    private static BookingEmailOptionsDTO CreateCompleteOptions() => new()
    {
        SmtpHost = "smtp.example.test",
        SmtpPort = 587,
        SmtpUsername = "smtp-user",
        SmtpPassword = "smtp-password",
        FromAddress = "bookings@example.test",
        FromName = "Custom Tours",
        ToAddress = "naren000000000@gmail.com"
    };

    /// <summary>
    /// A custom implementation of IBookingEmailTransport that captures the details of email messages sent during testing.
    /// </summary>
    private sealed class CapturingEmailTransport : IBookingEmailTransport
    {

        /// <summary>
        /// Gets the SMTP host used in the last email sent, allowing verification of the SMTP configuration in unit tests.
        /// </summary>
        public string? Host { get; private set; }

        /// <summary>
        /// Gets the SMTP port used in the last email sent, allowing verification of the SMTP configuration in unit tests.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the username used for SMTP authentication in the last email sent, allowing verification of the SMTP credentials in unit tests.
        /// </summary>
        public string? Username { get; private set; }

        /// <summary>
        /// Gets the sender's email address used in the last email sent, allowing verification of the "From" address in unit tests.
        /// </summary>
        public string? FromAddress { get; private set; }

        /// <summary>
        /// Gets the sender's display name used in the last email sent, allowing verification of the "From" name in unit tests.
        /// </summary>
        public string? FromName { get; private set; }

        /// <summary>
        /// Gets the recipient's email address used in the last email sent, allowing verification of the "To" address in unit tests.
        /// </summary>
        public string? ToAddress { get; private set; }

        /// <summary>
        /// Gets the subject of the last email sent, allowing verification of the email subject in unit tests.
        /// </summary>
        public string? Subject { get; private set; }

        /// <summary>
        /// Gets the body content of the last email sent, allowing verification of the email body in unit tests.
        /// </summary>
        public string? Body { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the body of the last email sent is in HTML format, allowing verification of the email content type in unit tests.
        /// </summary>
        public bool IsBodyHtml { get; private set; }

        /// <summary>
        /// Gets the number of times the SendAsync method has been called, allowing verification of how many emails were sent during testing.
        /// </summary>
        public int SendCount { get; private set; }

        /// <summary>
        /// Sends an email message asynchronously & captures the details of the message for verification.
        /// </summary>
        /// <param name="host">The SMTP host.</param>
        /// <param name="port">The SMTP port.</param>
        /// <param name="credentials">The SMTP credentials.</param>
        /// <param name="message">The email message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SendAsync(string host, int port, NetworkCredential credentials, MailMessage message)
        {
            Host = host;
            Port = port;
            Username = credentials.UserName;
            FromAddress = message.From!.Address;
            FromName = message.From.DisplayName;
            ToAddress = message.To.Single().Address;
            Subject = message.Subject;
            Body = message.Body;
            IsBodyHtml = message.IsBodyHtml;
            SendCount++;
            return Task.CompletedTask;
        }
    }
}
