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

/// <summary>
/// Integration tests for the booking request creation endpoint & its associated notification features.
/// </summary>
[TestFixture]
public class BookingNotificationIntegrationTests
{

    /// <summary>
    /// Tests that a booking request can be created successfully & that both WhatsApp & email notifications are attempted in parallel.
    /// </summary>
    [Test]
    public async Task CreateBookingRequest_SavesBookingAndStartsBothNotificationsInParallel() => await AssertBookingCreatedAndNotificationsAttemptedAsync(whatsappFails: false, emailFails: false);

    /// <summary>
    /// Tests that if the WhatsApp notification fails, the email notification still runs & the booking request is saved successfully.
    /// </summary>
    [Test]
    public async Task CreateBookingRequest_WhenWhatsAppFails_EmailStillRunsAndBookingIsSaved() => await AssertBookingCreatedAndNotificationsAttemptedAsync(whatsappFails: true, emailFails: false);

    /// <summary>
    /// Tests that if the email notification fails, the WhatsApp notification still runs & the booking request is saved successfully.
    /// </summary>
    [Test]
    public async Task CreateBookingRequest_WhenEmailFails_WhatsAppStillRunsAndBookingIsSaved() => await AssertBookingCreatedAndNotificationsAttemptedAsync(whatsappFails: false, emailFails: true);

    /// <summary>
    /// Tests that when both notification features are disabled via feature flags, the booking request is created successfully without attempting any notifications.
    /// </summary>
    [Test]
    public async Task CreateBookingRequest_WhenBothNotificationFeaturesAreDisabled_DoesNotAttemptNotifications()
    {
        await using var factory = new NotificationTestFactory(whatsappFails: false, emailFails: false, whatsappEnabled: false, emailEnabled: false);
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(factory.WhatsAppNotifier.Requests, Is.Empty);
            Assert.That(factory.EmailNotifier.Requests, Is.Empty);
        }

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TourismDbContext>();
        Assert.That(await dbContext.BookingRequests.CountAsync(), Is.EqualTo(1));
    }

    /// <summary>
    /// Asserts that a booking request is created successfully & that both WhatsApp & email notifications are attempted in parallel
    /// </summary>
    /// <param name="whatsappFails">A value indicating whether the WhatsApp notification method should simulate a failure.</param>
    /// <param name="emailFails">A value indicating whether the email notification method should simulate a failure.</param>
    private static async Task AssertBookingCreatedAndNotificationsAttemptedAsync(bool whatsappFails, bool emailFails) {
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(savedBooking.Id, Is.GreaterThan(0));
            Assert.That(savedBooking.FullName, Is.EqualTo("Integration Test"));
            Assert.That(factory.WhatsAppNotifier.Requests, Has.Count.EqualTo(1));
            Assert.That(factory.EmailNotifier.Requests, Has.Count.EqualTo(1));
            Assert.That(factory.Barrier.StartedCount, Is.EqualTo(2));
        }

    }

    /// <summary>
    /// A custom WebApplicationFactory that configures the test host with isolated in-memory database & test doubles for notification services.
    /// </summary>
    private sealed class NotificationTestFactory : WebApplicationFactory<Program>
    {

        /// <summary>
        /// A unique database name for the in-memory database used in this test factory instance, ensuring isolation between tests & preventing cross-test data pollution.
        /// </summary>
        private readonly string _databaseName = $"BookingNotificationTests_{Guid.NewGuid()}";

        /// <summary>
        /// A synchronization barrier that allows the test to assert that both notification methods were initiated in parallel before releasing them to complete.
        /// </summary>
        public NotificationBarrier Barrier { get; } = new();

        /// <summary>
        /// The test double for the IWhatsAppNotifier that records booking requests & can simulate failures.
        /// </summary>
        public RecordingWhatsAppNotifier WhatsAppNotifier { get; }

        /// <summary>
        /// The test double for the IBookingEmailNotifier that records booking requests & can simulate failures.
        /// </summary>
        public RecordingBookingEmailNotifier EmailNotifier { get; }

        /// <summary>
        /// Initializes a new instance of the NotificationTestFactory with the specified failure simulation settings for WhatsApp & email notifications.
        /// </summary>
        /// <param name="whatsappFails">A value indicating whether the WhatsApp notification method should simulate a failure.</param>
        /// <param name="emailFails">A value indicating whether the email notification method should simulate a failure.</param>
        /// <param name="whatsappEnabled">A value indicating whether the WhatsApp notification method is enabled.</param>
        /// <param name="emailEnabled">A value indicating whether the email notification method is enabled.</param>
        public NotificationTestFactory(bool whatsappFails, bool emailFails, bool whatsappEnabled = true, bool emailEnabled = true)
        {
            WhatsAppNotifier = new RecordingWhatsAppNotifier(Barrier);
            EmailNotifier = new RecordingBookingEmailNotifier(Barrier);
            WhatsAppNotifier.ShouldFail = whatsappFails;
            EmailNotifier.ShouldFail = emailFails;
            WhatsAppEnabled = whatsappEnabled;
            EmailEnabled = emailEnabled;
        }

        /// <summary>
        /// Configures the test web host with testing configuration, isolated in-memory database, & test doubles for notification services.
        /// </summary>
        private bool WhatsAppEnabled { get; }

        /// <summary>
        /// Configures the test web host with testing configuration, isolated in-memory database, & test doubles for notification services.
        /// </summary>
        private bool EmailEnabled { get; }

        /// <summary>
        /// Configures the test web host with testing configuration, isolated in-memory database, & test doubles for notification services.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
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
                services.AddDbContext<TourismDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    /// <summary>
    /// A synchronization barrier that allows two notification tasks to start in parallel & wait for a release signal before proceeding.
    /// </summary>
    private sealed class NotificationBarrier
    {

        /// <summary>
        /// A TaskCompletionSource that signals when both notification tasks have started & are waiting at the barrier.
        /// </summary>
        private readonly TaskCompletionSource _bothStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// A TaskCompletionSource that signals when the barrier is released, allowing both notification tasks to proceed.
        /// </summary>
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// A thread-safe counter that tracks how many notification tasks have reached the barrier.
        /// </summary>
        private int _startedCount;

        /// <summary>
        /// A TaskCompletionSource that signals when both notification tasks have started & are waiting at the barrier.
        /// </summary>
        public TaskCompletionSource BothNotificationsStarted => _bothStarted;

        /// <summary>
        /// A thread-safe property that returns the number of notification tasks that have reached the barrier.
        /// </summary>
        public int StartedCount => Volatile.Read(ref _startedCount);

        /// <summary>
        /// Waits asynchronously until both notification tasks have reached the barrier & then waits for the release signal.
        /// </summary>
        public async Task WaitUntilReleasedAsync()
        {
            if (Interlocked.Increment(ref _startedCount) == 2) _bothStarted.TrySetResult();

            await _release.Task;
        }

        /// <summary>
        /// Releases the barrier, allowing both notification tasks to proceed.
        /// </summary>
        public void ReleaseNotifications() => _release.TrySetResult();
    }

    /// <summary>
    /// A test double for the IWhatsAppNotifier that records booking requests & can simulate failures.
    /// </summary>
    /// <param name="barrier">The notification barrier to use.</param>
    private sealed class RecordingWhatsAppNotifier(NotificationBarrier barrier) : IWhatsAppNotifier {

        /// <summary>
        /// A thread-safe queue that records all booking requests for which the WhatsApp notification method is called.
        /// </summary>
        public ConcurrentQueue<BookingRequest> Requests { get; } = new();

        /// <summary>
        /// A flag indicating whether the WhatsApp notification method should simulate a failure by throwing an HttpRequestException.
        /// </summary>
        public bool ShouldFail { get; set; }

        /// <summary>
        /// Simulates sending a WhatsApp notification for a booking request. Records the booking request & waits for the barrier to be released.
        /// </summary>
        /// <param name="bookingRequest">The booking request for which to send a notification.</param>
        /// <exception cref="HttpRequestException">Thrown when the WhatsApp notification fails.</exception>
        public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
        {
            Requests.Enqueue(bookingRequest);
            await barrier.WaitUntilReleasedAsync();

            if (ShouldFail) throw new HttpRequestException("Simulated Meta API failure.");
        }
    }

    /// <summary>
    /// A test double for the IBookingEmailNotifier that records booking requests & can simulate failures.
    /// </summary>
    /// <param name="barrier">The notification barrier to use.</param>
    private sealed class RecordingBookingEmailNotifier(NotificationBarrier barrier) : IBookingEmailNotifier {

        /// <summary>
        /// A thread-safe queue that records all booking requests for which the email notification method is called.
        /// </summary>
        public ConcurrentQueue<BookingRequest> Requests { get; } = new();

        /// <summary>
        /// A flag indicating whether the email notification method should simulate a failure by throwing an SmtpException.
        /// </summary>
        public bool ShouldFail { get; set; }

        /// <summary>
        /// Simulates sending a booking notification email. Records the booking request & waits for the barrier to be released.
        /// </summary>
        /// <param name="bookingRequest">The booking request for which to send a notification email.</param>
        /// <exception cref="SmtpException">Thrown when the email notification fails.</exception>
        public async Task SendBookingNotificationAsync(BookingRequest bookingRequest)
        {
            Requests.Enqueue(bookingRequest);
            await barrier.WaitUntilReleasedAsync();

            if (ShouldFail) throw new SmtpException("Simulated SMTP failure.");
        }
    }
}
