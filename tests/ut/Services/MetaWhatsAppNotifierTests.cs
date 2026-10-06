using System.Net;
using System.Text.Json;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ut.Services;

[TestFixture]
public class MetaWhatsAppNotifierTests
{
    [Test]
    public async Task SendBookingNotificationAsync_SendsApprovedTemplateToConfiguredRecipient()
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var options = Options.Create(CreateCompleteOptions());
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            options,
            NullLogger<MetaWhatsAppNotifier>.Instance);

        var booking = new BookingRequest
        {
            Id = 42,
            FullName = "Ada Lovelace",
            PhoneNumber = "+441234567890",
            CurrentLocation = "London",
            Destination = "Pune",
            FromDate = new DateOnly(2027, 1, 2),
            ToDate = new DateOnly(2027, 1, 9),
            NumberOfMembers = 3,
            SpecialRequests = "Window seat"
        };

        await notifier.SendBookingNotificationAsync(booking);

        Assert.That(handler.RequestUri, Is.EqualTo(
            new Uri("https://graph.facebook.com/v26.0/123456789/messages")));
        Assert.That(handler.AuthorizationScheme, Is.EqualTo("Bearer"));
        Assert.That(handler.AuthorizationParameter, Is.EqualTo("test-token"));

        using var payload = JsonDocument.Parse(handler.RequestBody!);
        var root = payload.RootElement;
        Assert.That(root.GetProperty("messaging_product").GetString(), Is.EqualTo("whatsapp"));
        Assert.That(root.GetProperty("recipient_type").GetString(), Is.EqualTo("individual"));
        Assert.That(root.GetProperty("to").GetString(), Is.EqualTo("918428558275"));
        Assert.That(root.GetProperty("type").GetString(), Is.EqualTo("template"));
        var template = root.GetProperty("template");
        Assert.That(template.GetProperty("name").GetString(), Is.EqualTo("new_booking_request"));
        Assert.That(
            template.GetProperty("language").GetProperty("code").GetString(),
            Is.EqualTo("en_US"));

        var components = template.GetProperty("components");
        Assert.That(components.GetArrayLength(), Is.EqualTo(1));
        Assert.That(components[0].GetProperty("type").GetString(), Is.EqualTo("body"));
        var parameters = components[0].GetProperty("parameters");
        Assert.That(parameters.GetArrayLength(), Is.EqualTo(9));
        var expectedParameters = new[]
        {
            "42",
            "Ada Lovelace",
            "+441234567890",
            "London",
            "Pune",
            "2027-01-02",
            "2027-01-09",
            "3",
            "Window seat"
        };
        for (var index = 0; index < expectedParameters.Length; index++)
        {
            Assert.That(parameters[index].GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(parameters[index].GetProperty("text").GetString(), Is.EqualTo(expectedParameters[index]));
        }
    }

    [Test]
    public async Task SendBookingNotificationAsync_WhenSpecialRequestsAreMissing_UsesDefaultText()
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(CreateCompleteOptions()),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest { SpecialRequests = null });

        using var payload = JsonDocument.Parse(handler.RequestBody!);
        var parameters = payload.RootElement
            .GetProperty("template")
            .GetProperty("components")[0]
            .GetProperty("parameters");
        Assert.That(parameters[8].GetProperty("text").GetString(), Is.EqualTo("None"));
    }

    [Test]
    public async Task SendBookingNotificationAsync_WhenRecipientHasNoPlusPrefix_SendsToUnchangedNumber()
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var options = CreateCompleteOptions();
        options.WhatsAppTo = "918428558275";
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(options),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest());

        using var payload = JsonDocument.Parse(handler.RequestBody!);
        Assert.That(payload.RootElement.GetProperty("to").GetString(), Is.EqualTo("918428558275"));
    }

    [Test]
    public async Task SendBookingNotificationAsync_FlattensStandaloneCarriageReturnsAndLineFeeds()
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(CreateCompleteOptions()),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest
        {
            FullName = "Ada\nLovelace",
            PhoneNumber = "123\r456"
        });

        using var payload = JsonDocument.Parse(handler.RequestBody!);
        var parameters = payload.RootElement
            .GetProperty("template")
            .GetProperty("components")[0]
            .GetProperty("parameters");
        Assert.That(parameters[1].GetProperty("text").GetString(), Is.EqualTo("Ada Lovelace"));
        Assert.That(parameters[2].GetProperty("text").GetString(), Is.EqualTo("123 456"));
    }

    [TestCase("AccessToken")]
    [TestCase("PhoneNumberId")]
    [TestCase("WhatsAppTo")]
    [TestCase("GraphApiVersion")]
    [TestCase("TemplateName")]
    [TestCase("TemplateLanguageCode")]
    public async Task SendBookingNotificationAsync_WhenARequiredSettingIsMissing_DoesNotSend(
        string missingSetting)
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var options = CreateCompleteOptions();
        switch (missingSetting)
        {
            case "AccessToken":
                options.AccessToken = string.Empty;
                break;
            case "PhoneNumberId":
                options.PhoneNumberId = string.Empty;
                break;
            case "WhatsAppTo":
                options.WhatsAppTo = string.Empty;
                break;
            case "GraphApiVersion":
                options.GraphApiVersion = string.Empty;
                break;
            case "TemplateName":
                options.TemplateName = string.Empty;
                break;
            case "TemplateLanguageCode":
                options.TemplateLanguageCode = string.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(missingSetting));
        }
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(options),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        await notifier.SendBookingNotificationAsync(new BookingRequest());

        Assert.That(handler.RequestUri, Is.Null);
    }

    [TestCase("+")]
    [TestCase("++918428558275")]
    [TestCase("+91abc")]
    [TestCase("91abc1")]
    public void SendBookingNotificationAsync_WhenRecipientIsNotAnInternationalDigitNumber_Throws(
        string recipient)
    {
        var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var options = CreateCompleteOptions();
        options.WhatsAppTo = recipient;
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(options),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await notifier.SendBookingNotificationAsync(new BookingRequest()));
        Assert.That(
            exception.Message,
            Is.EqualTo("Meta WhatsApp recipient must be an international phone number containing digits only, optionally prefixed with '+'."));
        Assert.That(handler.RequestUri, Is.Null);
    }

    [Test]
    public void SendBookingNotificationAsync_WhenMetaRejectsRequest_ThrowsHttpRequestException()
    {
        var handler = new CapturingHttpMessageHandler(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(handler);
        var notifier = new MetaWhatsAppNotifier(
            httpClient,
            Options.Create(new MetaWhatsAppOptions
            {
                AccessToken = "test-token",
                PhoneNumberId = "123456789",
                WhatsAppTo = "918428558275",
                GraphApiVersion = "v26.0",
                TemplateName = "new_booking_request",
                TemplateLanguageCode = "en_US"
            }),
            NullLogger<MetaWhatsAppNotifier>.Instance);

        Assert.ThrowsAsync<HttpRequestException>(
            async () => await notifier.SendBookingNotificationAsync(new BookingRequest()));
    }

    [Test]
    public void MetaWhatsAppOptions_HaveEmptyDefaults()
    {
        var options = new MetaWhatsAppOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(options.AccessToken, Is.Empty);
            Assert.That(options.PhoneNumberId, Is.Empty);
            Assert.That(options.WhatsAppTo, Is.Empty);
            Assert.That(options.GraphApiVersion, Is.Empty);
            Assert.That(options.TemplateName, Is.Empty);
            Assert.That(options.TemplateLanguageCode, Is.Empty);
        }
    }

    private static MetaWhatsAppOptions CreateCompleteOptions() => new()
    {
        AccessToken = "test-token",
        PhoneNumberId = "123456789",
        WhatsAppTo = "+918428558275",
        GraphApiVersion = "v26.0",
        TemplateName = "new_booking_request",
        TemplateLanguageCode = "en_US"
    };

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public CapturingHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        public Uri? RequestUri { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode);
        }
    }
}
