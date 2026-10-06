using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using api.Datas;
using api.DTOs;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace it;

[TestFixture]
public class BookingNotificationIntegrationTests
{
    [Test]
    public async Task CreateBookingRequest_SavesBookingAndStartsBothNotificationsInParallel()
    {
        await AssertBookingCreatedAndNotificationsAttemptedAsync(
            whatsappFails: false,
            emailFails: false);
    }

    [Test]
    public async Task CreateBookingRequest_WhenWhatsAppFails_EmailStillRunsAndBookingIsSaved()
    {
        await AssertBookingCreatedAndNotificationsAttemptedAsync(
            whatsappFails: true,
            emailFails: false);
    }

    [Test]
    public async Task CreateBookingRequest_WhenEmailFails_WhatsAppStillRunsAndBookingIsSaved()
    {
        await AssertBookingCreatedAndNotificationsAttemptedAsync(
            whatsappFails: false,
            emailFails: true);
    }

    [Test]
    public async Task CreateBookingRequest_WhenBothNotificationFeaturesAreDisabled_DoesNotAttemptNotifications()
    {
        await using var factory = new NotificationTestFactory(
            whatsappFails: false,
            emailFails: false,
            whatsappEnabled: false,
            emailEnabled: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/v1/booking-requests",
            new BookingRequestDTO(
                "Feature Flag Test",
                "+918428558275",
                "London",
                "Pune",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35)),
                2,
                null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(factory.WhatsAppNotifier.Requests, Is.Empty);
        Assert.That(factory.EmailNotifier.Requests, Is.Empty);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
        Assert.That(await dbContext.BookingRequests.CountAsync(), Is.EqualTo(1));
    }

    private static async Task AssertBookingCreatedAndNotificationsAttemptedAsync(
        bool whatsappFails,
        bool emailFails)
    {
        await using var factory = new NotificationTestFactory(whatsappFails, emailFails);
        using var client = factory.CreateClient();

        var requestTask = client.PostAsJsonAsync(
            "/api/v1/booking-requests",
            new BookingRequestDTO(
                "Integration Test",
                "+918428558275",
                "London",
                "Pune",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35)),
                2,
                "Integration notification test"));

        try
        {
            await factory.Barrier.BothNotificationsStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            factory.Barrier.ReleaseNotifications();
        }

        using var response = await requestTask;
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
        var savedBooking = await dbContext.BookingRequests.AsNoTracking().SingleAsync();

        Assert.That(savedBooking.Id, Is.GreaterThan(0));
        Assert.That(savedBooking.FullName, Is.EqualTo("Integration Test"));
        Assert.That(factory.WhatsAppNotifier.Requests, Has.Count.EqualTo(1));
        Assert.That(factory.EmailNotifier.Requests, Has.Count.EqualTo(1));
        Assert.That(factory.Barrier.StartedCount, Is.EqualTo(2));
    }

    private sealed class NotificationTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"BookingNotificationTests_{Guid.NewGuid()}";

        public NotificationBarrier Barrier { get; } = new();

        public RecordingWhatsAppNotifier WhatsAppNotifier { get; }

        public RecordingBookingEmailNotifier EmailNotifier { get; }

        public NotificationTestFactory(
            bool whatsappFails,
            bool emailFails,
            bool whatsappEnabled = true,
            bool emailEnabled = true)
        {
            WhatsAppNotifier = new RecordingWhatsAppNotifier(Barrier);
            EmailNotifier = new RecordingBookingEmailNotifier(Barrier);
            WhatsAppNotifier.ShouldFail = whatsappFails;
            EmailNotifier.ShouldFail = emailFails;
            WhatsAppEnabled = whatsappEnabled;
            EmailEnabled = emailEnabled;
        }

        private bool WhatsAppEnabled { get; }

        private bool EmailEnabled { get; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BookingNotifications:WhatsAppEnabled"] = WhatsAppEnabled.ToString(),
                    ["BookingNotifications:EmailEnabled"] = EmailEnabled.ToString()
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IWhatsAppNotifier>();
                services.RemoveAll<IBookingEmailNotifier>();
                services.AddSingleton<IWhatsAppNotifier>(WhatsAppNotifier);
                services.AddSingleton<IBookingEmailNotifier>(EmailNotifier);
                services.AddDbContext<TourismDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    private sealed class NotificationBarrier
    {
        private readonly TaskCompletionSource _bothStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _startedCount;

        public TaskCompletionSource BothNotificationsStarted => _bothStarted;

        public int StartedCount => Volatile.Read(ref _startedCount);

        public async Task WaitUntilReleasedAsync()
        {
            if (Interlocked.Increment(ref _startedCount) == 2)
                _bothStarted.TrySetResult();

            await _release.Task;
        }

        public void ReleaseNotifications() => _release.TrySetResult();
    }

    private sealed class RecordingWhatsAppNotifier(NotificationBarrier barrier)
        : IWhatsAppNotifier
    {
        public ConcurrentQueue<BookingRequest> Requests { get; } = new();

        public bool ShouldFail { get; set; }

        public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
        {
            Requests.Enqueue(bookingRequest);
            await barrier.WaitUntilReleasedAsync();

            if (ShouldFail)
                throw new HttpRequestException("Simulated Meta API failure.");
        }
    }

    private sealed class RecordingBookingEmailNotifier(NotificationBarrier barrier)
        : IBookingEmailNotifier
    {
        public ConcurrentQueue<BookingRequest> Requests { get; } = new();

        public bool ShouldFail { get; set; }

        public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
        {
            Requests.Enqueue(bookingRequest);
            await barrier.WaitUntilReleasedAsync();

            if (ShouldFail)
                throw new SmtpException("Simulated SMTP failure.");
        }
    }
}
