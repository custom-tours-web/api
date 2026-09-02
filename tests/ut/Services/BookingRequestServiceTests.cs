using api.Interfaces;
using api.Models;
using api.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

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
        _mockLogger = new Mock<ILogger<BookingRequestService>>();

        _service = new BookingRequestService(
            _mockMapper.Object,
            _mockRepository.Object,
            _mockLogger.Object
        );
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

        // Act
        var result = await _service.CreateBookingRequestAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(mappedEntity.Id));
            Assert.That(result.Message, Is.EqualTo("Booking request submitted successfully."));
            Assert.That(result.Status, Is.EqualTo(mappedEntity.Status.ToString()));
            Assert.That(result.CreatedAt, Is.EqualTo(mappedEntity.CreatedAt));
        }

        _mockRepository.Verify(r => r.AddAsync(mappedEntity), Times.Once);
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
}
