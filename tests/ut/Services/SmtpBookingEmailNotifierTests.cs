using System.Net;
using System.Net.Mail;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ut.Services;

[TestFixture]
public class SmtpBookingEmailNotifierTests
{
    [Test]
    public async Task SendBookingNotificationAsync_SendsBookingDetailsToConfiguredAddress()
    {
        var transport = new CapturingEmailTransport();
        var notifier = new SmtpBookingEmailNotifier(
            Options.Create(new BookingEmailOptions
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

        Assert.That(transport.Host, Is.EqualTo("smtp.example.test"));
        Assert.That(transport.Port, Is.EqualTo(587));
        Assert.That(transport.Username, Is.EqualTo("smtp-user"));
        Assert.That(transport.FromAddress, Is.EqualTo("bookings@example.test"));
        Assert.That(transport.FromName, Is.EqualTo("Custom Tours"));
        Assert.That(transport.ToAddress, Is.EqualTo("naren000000000@gmail.com"));
        Assert.That(transport.Subject, Is.EqualTo("New booking request #19"));
        Assert.That(transport.IsBodyHtml, Is.False);
        Assert.That(transport.Body, Does.Contain("New booking request #19"));
        Assert.That(transport.Body, Does.Contain("London Injected: value"));
        Assert.That(transport.Body, Does.Contain("Name: Ada Lovelace"));
        Assert.That(transport.Body, Does.Contain("Phone: +441234567890"));
        Assert.That(transport.Body, Does.Contain("Destination: Pune"));
        Assert.That(transport.Body, Does.Contain("Dates: 2027-01-02 to 2027-01-09"));
        Assert.That(transport.Body, Does.Contain("Members: 3"));
        Assert.That(transport.Body, Does.Contain("Special requests: Window seat"));
        Assert.That(transport.Body, Does.Not.Contain("\r\nInjected:"));
    }

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

    [Test]
    public async Task SendBookingNotificationAsync_WhenSpecialRequestsAreMissing_UsesDefaultText()
    {
        var transport = new CapturingEmailTransport();
        var notifier = CreateNotifier(transport);

        await notifier.SendBookingNotificationAsync(new BookingRequest { SpecialRequests = null });

        Assert.That(transport.Body, Does.Contain("Special requests: None"));
    }

    [TestCase("SmtpHost")]
    [TestCase("SmtpUsername")]
    [TestCase("SmtpPassword")]
    [TestCase("FromAddress")]
    [TestCase("ToAddress")]
    public async Task SendBookingNotificationAsync_WhenARequiredSmtpSettingIsMissing_DoesNotSend(
        string missingSetting)
    {
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

        var notifier = new SmtpBookingEmailNotifier(
            Options.Create(options),
            transport,
            NullLogger<SmtpBookingEmailNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest());

        Assert.That(transport.SendCount, Is.Zero);
    }

    [Test]
    public void BookingEmailOptions_HaveExpectedDefaults()
    {
        var options = new BookingEmailOptions();

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

    private static SmtpBookingEmailNotifier CreateNotifier(CapturingEmailTransport transport) =>
        new(
            Options.Create(CreateCompleteOptions()),
            transport,
            NullLogger<SmtpBookingEmailNotifier>.Instance);

    private static BookingEmailOptions CreateCompleteOptions() => new()
    {
        SmtpHost = "smtp.example.test",
        SmtpPort = 587,
        SmtpUsername = "smtp-user",
        SmtpPassword = "smtp-password",
        FromAddress = "bookings@example.test",
        FromName = "Custom Tours",
        ToAddress = "naren000000000@gmail.com"
    };

    private sealed class CapturingEmailTransport : IBookingEmailTransport
    {
        public string? Host { get; private set; }

        public int Port { get; private set; }

        public string? Username { get; private set; }

        public string? FromAddress { get; private set; }

        public string? FromName { get; private set; }

        public string? ToAddress { get; private set; }

        public string? Subject { get; private set; }

        public string? Body { get; private set; }

        public bool IsBodyHtml { get; private set; }

        public int SendCount { get; private set; }

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
