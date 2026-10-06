using api.Interfaces;
using api.Models;
using api.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http;
using System.Net.Mail;

namespace ut.Services;

/// <summary>
/// Unit tests for the BookingRequestService class.
/// </summary>
[TestFixture]
public class BookingRequestServiceTests
{

    /// <summary>
    /// Mocked IMapper for the BookingRequestService to verify mapping behavior during tests.
    /// </summary>
    private Mock<IMapper> _mockMapper;

    /// <summary>
    /// Mocked IBookingRequestRepository for the BookingRequestService to verify repository interactions during tests.
    /// </summary>
    private Mock<IBookingRequestRepository> _mockRepository;

    /// <summary>
    /// Mocked WhatsApp notifier for verifying booking notification behavior.
    /// </summary>
    private Mock<IWhatsAppNotifier> _mockWhatsAppNotifier;

    /// <summary>
    /// Mocked email notifier for verifying booking notification behavior.
    /// </summary>
    private Mock<IBookingEmailNotifier> _mockBookingEmailNotifier;

    /// <summary>
    /// Mocked ILogger for the BookingRequestService to verify logging behavior during tests.
    /// </summary>
    private Mock<ILogger<BookingRequestService>> _mockLogger;

    /// <summary>
    /// This is the actual service implementation that contains the business logic for handling booking requests.
    /// </summary>
    private BookingRequestService _service;

    /// <summary>
    /// It initializes the mocked dependencies and creates a new instance of the BookingRequestService for testing.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepository = new Mock<IBookingRequestRepository>();
        _mockWhatsAppNotifier = new Mock<IWhatsAppNotifier>();
        _mockBookingEmailNotifier = new Mock<IBookingEmailNotifier>();
        _mockLogger = new Mock<ILogger<BookingRequestService>>();

        _service = new BookingRequestService(
            _mockMapper.Object,
            _mockRepository.Object,
            _mockWhatsAppNotifier.Object,
            _mockBookingEmailNotifier.Object,
            Options.Create(new BookingNotificationFeatures
            {
                WhatsAppEnabled = true,
                EmailEnabled = true
            }),
            _mockLogger.Object
        );
    }

    [Test]
    public async Task CreateBookingRequestAsync_WhenNotificationsAreDisabled_DoesNotInvokeNotifiers()
    {
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _service = new BookingRequestService(
            _mockMapper.Object,
            _mockRepository.Object,
            _mockWhatsAppNotifier.Object,
            _mockBookingEmailNotifier.Object,
            Options.Create(new BookingNotificationFeatures()),
            _mockLogger.Object);

        var result = await _service.CreateBookingRequestAsync(dto);

        Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
        _mockWhatsAppNotifier.Verify(
            n => n.SendBookingNotificationAsync(It.IsAny<BookingRequest>()),
            Times.Never);
        _mockBookingEmailNotifier.Verify(
            n => n.SendBookingNotificationAsync(It.IsAny<BookingRequest>()),
            Times.Never);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task CreateBookingRequestAsync_WhenOnlyOneNotificationIsEnabled_InvokesOnlyThatNotifier(
        bool whatsAppEnabled,
        bool emailEnabled)
    {
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _service = new BookingRequestService(
            _mockMapper.Object,
            _mockRepository.Object,
            _mockWhatsAppNotifier.Object,
            _mockBookingEmailNotifier.Object,
            Options.Create(new BookingNotificationFeatures
            {
                WhatsAppEnabled = whatsAppEnabled,
                EmailEnabled = emailEnabled
            }),
            _mockLogger.Object);

        await _service.CreateBookingRequestAsync(dto);

        _mockWhatsAppNotifier.Verify(
            notifier => notifier.SendBookingNotificationAsync(mappedEntity),
            whatsAppEnabled ? Times.Once() : Times.Never());
        _mockBookingEmailNotifier.Verify(
            notifier => notifier.SendBookingNotificationAsync(mappedEntity),
            emailEnabled ? Times.Once() : Times.Never());
    }

    [Test]
    public async Task CreateBookingRequestAsync_WaitsForEnabledNotificationsBeforeReturning()
    {
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();
        var whatsAppCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var emailCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(whatsAppCompletion.Task);
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(emailCompletion.Task);

        var resultTask = _service.CreateBookingRequestAsync(dto);

        Assert.That(resultTask.IsCompleted, Is.False);
        whatsAppCompletion.SetResult();
        emailCompletion.SetResult();
        var result = await resultTask;

        Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
    }

    [Test]
    public async Task CreateBookingRequestAsync_LogsCustomerNameWithoutLineBreaks()
    {
        var dto = BookingTestData.GetValidBookingRequestDTO() with
        {
            FullName = "Ada\r\nLovelace\nInjected"
        };
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();
        var logger = new CapturingLogger<BookingRequestService>();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _service = new BookingRequestService(
            _mockMapper.Object,
            _mockRepository.Object,
            _mockWhatsAppNotifier.Object,
            _mockBookingEmailNotifier.Object,
            Options.Create(new BookingNotificationFeatures
            {
                WhatsAppEnabled = true,
                EmailEnabled = true
            }),
            logger);

        await _service.CreateBookingRequestAsync(dto);

        Assert.That(
            logger.DebugMessage,
            Is.EqualTo("Starting creation of booking request for customer: AdaLovelaceInjected"));
    }

    /// <summary>
    /// It can be used to clean up resources or reset states if necessary.
    /// </summary>
    [Test]
    public async Task CreateBookingRequestAsync_Success_ReturnsBookingResponseDTO()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateBookingRequestAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
            Assert.That(result.Message, Is.EqualTo("Booking request submitted successfully."));
            Assert.That(result.Status, Is.EqualTo(mappedEntity.Status.ToString()));
            Assert.That(result.CreatedAt, Is.GreaterThan(DateTimeOffset.UnixEpoch));
        }

        _mockRepository.Verify(r => r.AddAsync(mappedEntity), Times.Once);
        _mockWhatsAppNotifier.Verify(n => n.SendBookingNotificationAsync(mappedEntity), Times.Once);
        _mockBookingEmailNotifier.Verify(n => n.SendBookingNotificationAsync(mappedEntity), Times.Once);
    }

    [Test]
    public async Task CreateBookingRequestAsync_WhenWhatsAppFails_ReturnsSuccessfulBookingResponse()
    {
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .ThrowsAsync(new HttpRequestException("Meta WhatsApp API unavailable"));
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateBookingRequestAsync(dto);

        Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
        _mockRepository.Verify(r => r.AddAsync(mappedEntity), Times.Once);
        _mockBookingEmailNotifier.Verify(n => n.SendBookingNotificationAsync(mappedEntity), Times.Once);
    }

    [Test]
    public async Task CreateBookingRequestAsync_WhenEmailFails_ReturnsSuccessfulBookingResponse()
    {
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).Returns(Task.CompletedTask);
        _mockWhatsAppNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .Returns(Task.CompletedTask);
        _mockBookingEmailNotifier
            .Setup(n => n.SendBookingNotificationAsync(mappedEntity))
            .ThrowsAsync(new SmtpException("SMTP unavailable"));

        var result = await _service.CreateBookingRequestAsync(dto);

        Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
        _mockWhatsAppNotifier.Verify(n => n.SendBookingNotificationAsync(mappedEntity), Times.Once);
    }

    /// <summary>
    /// Test to verify that the CreateBookingRequestAsync method of the BookingRequestService correctly handles exceptions thrown by the mapper.
    /// </summary>
    [Test]
    public void CreateBookingRequestAsync_WhenMapperThrows_LogsAndRethrowsException()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var expectedException = new AutoMapperMappingException("Mapping failed");

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Throws(expectedException);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AutoMapperMappingException>(
            async () => await _service.CreateBookingRequestAsync(dto));

        Assert.That(ex.Message, Is.EqualTo("Mapping failed"));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<BookingRequest>()), Times.Never);
    }

    /// <summary>
    /// Test to verify that the CreateBookingRequestAsync method of the BookingRequestService correctly handles exceptions thrown by the repository.
    /// </summary>
    [Test]
    public void CreateBookingRequestAsync_WhenRepositoryThrows_LogsAndRethrowsException()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var mappedEntity = BookingTestData.GetValidBookingRequestEntity();

        var expectedException = new InvalidOperationException("Database connection failed");

        _mockMapper.Setup(m => m.Map<BookingRequest>(dto)).Returns(mappedEntity);
        _mockRepository.Setup(r => r.AddAsync(mappedEntity)).ThrowsAsync(expectedException);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateBookingRequestAsync(dto));

        Assert.That(ex.Message, Is.EqualTo("Database connection failed"));

        _mockMapper.Verify(m => m.Map<BookingRequest>(dto), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(mappedEntity), Times.Once);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public string? DebugMessage { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
                DebugMessage = formatter(state, exception);
        }
    }
}
